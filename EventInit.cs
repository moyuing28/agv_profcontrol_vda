namespace ProfControl
{


    public partial class VDA5050 : IAppScript
    {
        public double ConvertCwToCcw180(double angleCw360)
        {
            // ① 映射到[0,360)
            angleCw360 = (angleCw360 % 360 + 360) % 360;

            // ② 顺时针 → 逆时针（取反）
            double angleCcw = -angleCw360;

            // ③ 映射到[-180,180)
            if (angleCcw < -180)
                angleCcw += 360;

            return angleCcw;
        }

        public void EventAdd()
        {
            foreach (var agv in API.Agvs)
            {
                agv.GetNewRoutesEvent += Agv_GetNewRoutesEvent;
                //    agv.EnterNewRouteEvent += Agv_EnterNewRouteEvent;
                agv.DoActionEvent += Agv_DoActionEvent;
                //  agv.RouteFinishedEvent += Agv_RouteFinishedEvent;
            }
        }

        public void EventKill()
        {
            foreach (var agv in API.Agvs)
            {
                agv.GetNewRoutesEvent -= Agv_GetNewRoutesEvent;
                //  agv.EnterNewRouteEvent -= Agv_EnterNewRouteEvent;
                agv.DoActionEvent -= Agv_DoActionEvent;
                //  agv.RouteFinishedEvent -= Agv_RouteFinishedEvent;
            }

        }

        //得到可以导航的全部路径信息
        private void Agv_GetNewRoutesEvent(object sender, GetNewRoutesEventArgs e)
        {
            Tools.trace("获取到新导航路径");

            var agv = sender as IAgv;
            var routes = e.Route;


            var manufacturer = agv.GetValue<string>("manufacturer");
            var serialNumber = agv.GetValue<string>("serialNumber");
            var versionStr = agv.GetValue<string>("version");

            var jobj = new JObject();
            jobj.Add(new JProperty("headerId", taskId));
            jobj.Add(new JProperty("timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffZ")));
            jobj.Add(new JProperty("version", versionStr));
            jobj.Add(new JProperty("manufacturer", manufacturer));
            jobj.Add(new JProperty("serialNumber", serialNumber));
            jobj.Add(new JProperty("orderId", "order-" + taskId.ToString()));
            jobj.Add(new JProperty("orderUpdateId", (uint)0));
            //jobj.Add(new JProperty("zoneSetId", "zoneSetId"));

            var nodes = new JArray();
            var edges = new JArray();

            var idx = 0;

//            // 构建巷道入口站点集合（子分类"入口"，需在规划中预先分配）
//            var entranceStationMarks = new HashSet<string>();
//            var entranceSub = SubCategoryCollection.Instance["入口"] as ISubCategory;
//            if (entranceSub != null)
//            {
//                foreach (IStation sta in entranceSub.Children)
//                    entranceStationMarks.Add(sta.MarkPointValueMirror);
//            }

            // 构建车库位站点集合（子分类"车库位"，需在规划中预先分配）
            var garageStationMarks = new HashSet<string>();
            var garageSub = SubCategoryCollection.Instance["车库位"] as ISubCategory;
            if (garageSub != null)
            {
                foreach (IStation sta in garageSub.Children)
                    garageStationMarks.Add(sta.MarkPointValueMirror);
            }

            foreach (var route in routes)
            {
                var jNode = new JObject();
                jNode.Add(new JProperty("nodeId", route.InStation.MarkPointValueMirror));
                jNode.Add(new JProperty("sequenceId", idx++));
                jNode.Add(new JProperty("nodeDescription", string.Empty));
                jNode.Add(new JProperty("released", true));
                var jActions = new JArray();
                var jAction = new JObject();
                //                jAction.Add(new JProperty("actionType", ""));
                //                jAction.Add(new JProperty("actionId", ""));
                //                jAction.Add(new JProperty("actionDescription", ""));
                //                jAction.Add(new JProperty("blockingType", ""));
                //                jAction.Add(new JProperty("actionParameters", new JArray()));

                jNode.Add(new JProperty("actions", jActions)); // for node's actions

//                // 如果该节点是巷道入口站（属于子分类"入口"），添加"进入巷道"动作
//                if (entranceStationMarks.Contains(route.InStation.MarkPointValueMirror))
//                {
//                    var enterAction = new JObject();
//                    string enterActionId = actionId.ToString();
//                    actionId++;
//                    var enterActionType = "detectObject";
//
//                    enterAction.Add(new JProperty("actionType", enterActionType));
//                    enterAction.Add(new JProperty("actionId", enterActionId));
//                    enterAction.Add(new JProperty("actionDescription", ""));
//                    enterAction.Add(new JProperty("blockingType", "HARD"));
//                    enterAction.Add(new JProperty("actionParameters", new JArray()));
//                    jActions.Add(enterAction);
//                }

                var jNodePosition = new JObject();
                double dx = route.InStation.RealCoordinate.X;
                double dy = route.InStation.RealCoordinate.Y;

                jNodePosition.Add(new JProperty("x", dx));
                jNodePosition.Add(new JProperty("y", dy));



                //jNodePosition.Add(new JProperty("theta", route.PathDir.InAngle.AngleToRadian()));
                jNodePosition.Add(new JProperty("theta", 0));
                double allowedDeviationXY = agv.GetValue<double>("allowedDeviationXY");
                double allowedDeviationTheta = agv.GetValue<double>("allowedDeviationTheta");
                allowedDeviationTheta = allowedDeviationTheta * (Math.PI / 180.0); // angle->rad
                jNodePosition.Add(new JProperty("allowedDeviationXY", allowedDeviationXY));
                jNodePosition.Add(new JProperty("allowedDeviationTheta", allowedDeviationTheta));
                //                jNodePosition.Add(new JProperty("mapId", ""));
                //                jNodePosition.Add(new JProperty("mapDescription", ""));
                jNode.Add(new JProperty("nodePosition", jNodePosition));

                nodes.Add(jNode);


                var jEdge = new JObject();
                jEdge.Add(new JProperty("edgeId", route.Path.DefaultName));
                jEdge.Add(new JProperty("sequenceId", idx++));

                var edgeDescription = string.Empty;
                if (garageStationMarks.Contains(route.PathDir.InStation.MarkPointValueMirror)
                         || garageStationMarks.Contains(route.PathDir.OutStation.MarkPointValueMirror))
                {
                    // 该边任一端站点属于车库位，夹在车库位之间的边即为巷道
                    edgeDescription = "is_lane";
                }
                jEdge.Add(new JProperty("edgeDescription", edgeDescription));

                jEdge.Add(new JProperty("released", true));
                jEdge.Add(new JProperty("startNodeId", route.PathDir.InStation.MarkPointValueMirror));
                jEdge.Add(new JProperty("endNodeId", route.PathDir.OutStation.MarkPointValueMirror));
                jEdge.Add(new JProperty("maxSpeed", (route.Path.Model as IPathModel).AgvSpeed));
                //                jEdge.Add(new JProperty("maxHeight", 100d));
                //                jEdge.Add(new JProperty("minHeight", 0d));

                double path_ori = route.Decision.WorkAngle ?? 0.0;
                double orientation = (path_ori == 0.0) ? 0.0 : Math.PI;
                jEdge.Add(new JProperty("orientation", orientation));

                jEdge.Add(new JProperty("orientationType", "TANGENTIAL"));
                //                jEdge.Add(new JProperty("direction", route.Decision.MaybeBranchMode)); // 这边要做岔路枚举映射
                jEdge.Add(new JProperty("rotationAllowed", false));
                //                jEdge.Add(new JProperty("maxRotationSpeed", "100"));

                jEdge.Add(new JProperty("length", route.Path.RealPathLength));

                jActions = new JArray();
                jAction = new JObject();
                //                jAction.Add(new JProperty("actionType", ""));
                //                jAction.Add(new JProperty("actionId", ""));
                //                jAction.Add(new JProperty("actionDescription", ""));
                //                jAction.Add(new JProperty("blockingType", ""));
                //                jAction.Add(new JProperty("actionParameters", new JArray()));
                jEdge.Add(new JProperty("actions", jActions)); // for edge's actions

                if (route.Path is LineView)
                {
                    var trajectory = new JObject();
                    jEdge.Add(new JProperty("trajectory", trajectory));
                }
                else if (route.Path is BezierView)
                {
                    var trajectory = new JObject();
                    trajectory.Add(new JProperty("degree", 3));
                    var knotVectorArray = new float[] { 0, 0, 0, 0, 1, 1, 1, 1 };
                    trajectory.Add(new JProperty("knotVector", knotVectorArray));
                    var controlPoints = new JArray();

                    var jPositionStart = new JObject();
                    jPositionStart.Add(new JProperty("x", route.InStation.RealCoordinate.X));
                    jPositionStart.Add(new JProperty("y", route.InStation.RealCoordinate.Y));
                    jPositionStart.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionStart);

                    var C1 = new Point(0, 0);
                    var C2 = new Point(0, 0);
                    if (route.InStation == route.Path.StartStation)
                    {
                        C1 = (route.Path as BezierView).C1;
                        C2 = (route.Path as BezierView).C2;
                    }
                    else
                    {
                        C1 = (route.Path as BezierView).C2;
                        C2 = (route.Path as BezierView).C1;
                    }
                    var jPositionC1 = new JObject();
                    jPositionC1.Add(new JProperty("x", C1.X));
                    jPositionC1.Add(new JProperty("y", C1.Y));
                    jPositionC1.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionC1);

                    var jPositionC2 = new JObject();
                    jPositionC2.Add(new JProperty("x", C2.X));
                    jPositionC2.Add(new JProperty("y", C2.Y));
                    jPositionC2.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionC2);

                    var jPositionEnd = new JObject();
                    jPositionEnd.Add(new JProperty("x", route.OutStation.RealCoordinate.X));
                    jPositionEnd.Add(new JProperty("y", route.OutStation.RealCoordinate.Y));
                    jPositionEnd.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionEnd);

                    trajectory.Add(new JProperty("controlPoints", controlPoints));

                    jEdge.Add(new JProperty("trajectory", trajectory));

                }
                else if (route.Path is ArcView)
                {
                    var trajectory = new JObject();
                    trajectory.Add(new JProperty("degree", 999));
                    var controlPoints = new JArray();

                    var jPositionStart = new JObject();
                    jPositionStart.Add(new JProperty("x", route.InStation.RealCoordinate.X));
                    jPositionStart.Add(new JProperty("y", route.InStation.RealCoordinate.Y));
                    jPositionStart.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionStart);


                    var Center = new Point(0, 0);
                    Center = (route.Path as ArcView).Center.ToWorldPoint();
                    var r = (route.Path as ArcView).Radius.ToWorldSize();

                    var jPositionCenter = new JObject();
                    jPositionCenter.Add(new JProperty("x", Center.Y - 1.611));
                    jPositionCenter.Add(new JProperty("y", Center.X - 3.418));
                    jPositionCenter.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionCenter);

                    var jPositionEnd = new JObject();
                    jPositionEnd.Add(new JProperty("x", route.OutStation.RealCoordinate.X));
                    jPositionEnd.Add(new JProperty("y", route.OutStation.RealCoordinate.Y));
                    jPositionEnd.Add(new JProperty("weight", 1.0));
                    controlPoints.Add(jPositionEnd);

                    trajectory.Add(new JProperty("controlPoints", controlPoints));
                    jEdge.Add(new JProperty("trajectory", trajectory));
                }
                else
                {
                    var trajectory = new JObject();
                    jEdge.Add(new JProperty("trajectory", trajectory));
                }
                edges.Add(jEdge);

                if (route.Equals(routes[routes.Count - 1]))
                {
                    var JNodeLast = (JObject)jNode.DeepClone();
                    JNodeLast["nodeId"] = route.OutStation.MarkPointValueMirror;
                    JNodeLast["sequenceId"] = idx++;
                    JNodeLast["nodePosition"]["x"] = route.OutStation.RealCoordinate.X;
                    JNodeLast["nodePosition"]["y"] = route.OutStation.RealCoordinate.Y;
                    JNodeLast["nodePosition"]["theta"] = 0;

                    if (agv.GetValue<string>("staID") == route.OutStation.MarkPointValueMirror)
                    {
                        var jActions_lastnode = new JArray();
                        var jAction_lastnode = new JObject();
                        string actionIdStr = actionId.ToString();
                        agv.SetValue("returnstaID", route.InStation.MarkPointValueMirror);
                        var actiontype = agv.GetValue<string>("staAction");
                        jAction_lastnode.Add(new JProperty("actionType", actiontype));
                        jAction_lastnode.Add(new JProperty("actionId", actionIdStr));
                        actionId++;
                        jAction_lastnode.Add(new JProperty("actionDescription", ""));
                        jAction_lastnode.Add(new JProperty("blockingType", "HARD"));
                        jAction_lastnode.Add(new JProperty("actionParameters", new JArray()));
                        jActions_lastnode.Add(jAction_lastnode);
                        JNodeLast["actions"] = jActions_lastnode;
                    }
                    nodes.Add(JNodeLast);
                }
            }

            jobj.Add(new JProperty("nodes", nodes));
            jobj.Add(new JProperty("edges", edges));

            agv.DataParserFactory("Order路径", DataParser.DataDirection.Send, jobj);

            var jsonString = jobj.ToString();
            var data = Encoding.UTF8.GetBytes(jsonString);
            //Tools.trace(jsonString);
            API.MQTTCSend(clientId, $"profcontrol/{versionStr}/{manufacturer}/{serialNumber}/order", data, success =>
            {

            }, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            taskId++;
        }



        //动作执行事件
        private void Agv_DoActionEvent(object sender, DoActionEventArgs e)
        {
            Tools.trace("执行动作");
            var agv = sender as IAgv;

            var manufacturer = agv.GetValue<string>("manufacturer");
            var serialNumber = agv.GetValue<string>("serialNumber");
            var versionStr = agv.GetValue<string>("version");

            var jobj = new JObject();
            jobj.Add(new JProperty("headerId", instantactionId));
            jobj.Add(new JProperty("timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffZ")));
            jobj.Add(new JProperty("version", versionStr));
            jobj.Add(new JProperty("manufacturer", manufacturer));
            jobj.Add(new JProperty("serialNumber", serialNumber));
            if (e.ActionName == "暂停运动")
            {

                var jAction = new JObject();
                jAction.Add(new JProperty("actionType", "startPause"));
                jAction.Add(new JProperty("actionId", instantactionId.ToString()));
                jAction.Add(new JProperty("actionDescription", string.Empty));
                jAction.Add(new JProperty("actionStatus", "WAITING"));
                jAction.Add(new JProperty("resultDescription", string.Empty));

                var JActions = new JArray();
                JActions.Add(jAction);
                jobj.Add(new JProperty("actions", JActions));

                var jsonString = jobj.ToString();
                var data = Encoding.UTF8.GetBytes(jsonString);
                Tools.trace(jsonString);
                API.MQTTCSend(clientId, $"profcontrol/{versionStr}/{manufacturer}/{serialNumber}/instantActions", data, success =>
                {

                });

            }
            else if (e.ActionName == "恢复运动")
            {
                var jAction = new JObject();
                jAction.Add(new JProperty("actionType", "stopPause"));
                jAction.Add(new JProperty("actionId", instantactionId.ToString()));
                jAction.Add(new JProperty("actionDescription", string.Empty));
                jAction.Add(new JProperty("actionStatus", "WAITING"));
                jAction.Add(new JProperty("resultDescription", string.Empty));

                var JActions = new JArray();
                JActions.Add(jAction);
                jobj.Add(new JProperty("actions", JActions));

                var jsonString = jobj.ToString();
                var data = Encoding.UTF8.GetBytes(jsonString);
                Tools.trace(jsonString);
                API.MQTTCSend(clientId, $"profcontrol/{versionStr}/{manufacturer}/{serialNumber}/instantActions", data, success =>
                {

                });
            }
            instantactionId++;
        }

    }
}              