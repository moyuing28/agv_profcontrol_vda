namespace ProfControl
{
    public partial class VDA5050 : IAppScript
    {
        public void MqttInit()
        {

            API.MQTTS(1883, "", "",
            clientId =>
            {
                Tools.trace(String.Format("客户端上线:{0}", clientId));
            },
            clientId =>
            {
                Tools.trace(String.Format("客户端下线:{0}", clientId));
            },
            (clientId, topic) =>
            {
                Tools.trace(String.Format("新订阅:{0}, 主题:{1}", clientId, topic));
            },
            (clientId, topic) =>
            {
                Tools.trace(String.Format("删除订阅:{0}, 主题:{1}", clientId, topic));
            }
            );

            List<string> topicsToSubscribe = new List<string>();
            foreach (var agv in API.Agvs)
            {
                var manufacturer = agv.GetValue<string>("manufacturer");
                var serialNumber = agv.GetValue<string>("serialNumber");
                var versionStr = agv.GetValue<string>("version");
                var agv_id = agv.Name;
                if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(serialNumber))
                {
                    // 添加主题
                    topicsToSubscribe.Add($"profcontrol/{versionStr}/{manufacturer}/{serialNumber}/state");
                    topicsToSubscribe.Add($"profcontrol/{versionStr}/{manufacturer}/{serialNumber}/connection");
                    topicsToSubscribe.Add($"profcontrol/{versionStr}/{manufacturer}/{serialNumber}/factsheet");
                }
                else
                {
                    // 可选：记录哪些 AGV 数据不完整
                    Tools.trace($"跳过 AGV（缺少制造商或序列号）: {agv_id}");
                }
            }

            API.MQTTC("120.26.137.47", 1883, "", "", topicsToSubscribe, (topic, data) =>
            {
                if (topic.EndsWith("/factsheet"))
                {
                    try
                    {
                        var dataStr = Encoding.UTF8.GetString(data);
                        var json = JObject.Parse(dataStr);
                        //Tools.trace(dataStr);
                        var headerId = json["headerId"]?.ToObject<uint>();
                        var timestamp = json["timestamp"]?.ToObject<string>();
                        var version = json["version"]?.ToObject<string>();
                        var manufacturer = json["manufacturer"]?.ToObject<string>();
                        var serialNumber = json["serialNumber"]?.ToObject<string>();

                        var agv = API.GetAgv(serialNumber);
                        if (agv == null)
                            agv = API.Agvs.Where(x => x.GetValue<string>("serialNumber") == serialNumber).FirstOrDefault();
                        var typeSpecification = json["typeSpecification"] as JObject;
                        var physicalParameters = json["physicalParameters"] as JObject;
                        var protocolLimits = json["protocolLimits"] as JObject;
                        var protocolFeatures = json["protocolFeatures"] as JObject;
                        var agvGeometry = json["agvGeometry"] as JObject;
                        var loadSpecification = json["loadSpecification"] as JObject;
                        var localizationParameters = json["localizationParameters"] as JObject;

                        var seriesName = typeSpecification["seriesName"]?.ToObject<string>();
                        var seriesDescription = typeSpecification["seriesDescription"]?.ToObject<string>();
                        var agvKinematic = typeSpecification["agvKinematic"]?.ToObject<string>();
                        var agvClass = typeSpecification["agvClass"]?.ToObject<string>();
                        var maxLoadMass = typeSpecification["maxLoadMass"]?.ToObject<double>();
                        var localizationTypes = typeSpecification["localizationTypes"]?.ToObject<string[]>();
                        var navigationTypes = typeSpecification["navigationTypes"]?.ToObject<string[]>();

                        var speedMin = physicalParameters["speedMin"]?.ToObject<double>();
                        var speedMax = physicalParameters["speedMax"]?.ToObject<double>();
                        var accelerationMax = physicalParameters["accelerationMax"]?.ToObject<double>();
                        var decelerationMax = physicalParameters["decelerationMax"]?.ToObject<double>();
                        var heightMin = physicalParameters["heightMin"]?.ToObject<double>();
                        var heightMax = physicalParameters["heightMax"]?.ToObject<double>();
                        var width = physicalParameters["width"]?.ToObject<double>();
                        var length = physicalParameters["length"]?.ToObject<double>();

                        var maxStringLens = protocolLimits["maxStringLens"] as JObject;
                        var msgLen = maxStringLens["msgLen"]?.ToObject<uint>();
                        var topicSerialLen = maxStringLens["topicSerialLen"]?.ToObject<uint>();
                        var topicElemLen = maxStringLens["topicElemLen"]?.ToObject<uint>();
                        var idLen = maxStringLens["idLen"]?.ToObject<uint>();
                        var idNumericalOnly = maxStringLens["idNumericalOnly"]?.ToObject<bool>();
                        var enumLen = maxStringLens["enumLen"]?.ToObject<uint>();
                        var loadIdLen = maxStringLens["loadIdLen"]?.ToObject<uint>();

                        var maxArrayLens = protocolLimits["maxArrayLens"] as JObject;

                        var order_nodes = maxArrayLens["order.nodes"]?.ToObject<uint>();
                        var order_edges = maxArrayLens["order.edges"]?.ToObject<uint>();
                        var node_actions = maxArrayLens["node.actions"]?.ToObject<uint>();
                        var edge_actions = maxArrayLens["edge.actions"]?.ToObject<uint>();
                        var actions_actionsParameters = maxArrayLens["actions.actionsParameters"]?.ToObject<uint>();
                        var instantActions = maxArrayLens["instantActions"]?.ToObject<uint>();
                        var trajectory_knotVector = maxArrayLens["trajectory.knotVector"]?.ToObject<uint>();
                        var trajectory_controlPoints = maxArrayLens["trajectory.controlPoints"]?.ToObject<uint>();
                        var state_nodeStates = maxArrayLens["state.nodeStates"]?.ToObject<uint>();
                        var state_edgeStates = maxArrayLens["state.edgeStates"]?.ToObject<uint>();
                        var state_loads = maxArrayLens["state.loads"]?.ToObject<uint>();
                        var state_actionStates = maxArrayLens["state.actionStates"]?.ToObject<uint>();
                        var state_errors = maxArrayLens["state.errors"]?.ToObject<uint>();
                        var state_informations = maxArrayLens["state.informations"]?.ToObject<uint>();
                        var error_errorReferences = maxArrayLens["error.errorReferences"]?.ToObject<uint>();
                        var informations_infoReferences = maxArrayLens["informations.infoReferences"]?.ToObject<uint>();

                        var timing = protocolLimits["timing"] as JObject;
                        var minOrderInterval = timing["minOrderInterval"]?.ToObject<float>();
                        var minStateInterval = timing["minStateInterval"]?.ToObject<float>();
                        var defaultStateInterval = timing["defaultStateInterval"]?.ToObject<float>();
                        var visualizationInterval = timing["visualizationInterval"]?.ToObject<float>();

                        if (protocolFeatures["optionalParameters"] != null)
                        {
                            var optionalParameters = protocolFeatures["optionalParameters"] as JArray;
                            foreach (var optionalParameter in optionalParameters)
                            {
                                var parameter = optionalParameter["parameter"]?.ToObject<string>();
                                var support = optionalParameter["support"]?.ToObject<string>();
                                var description = optionalParameter["description"]?.ToObject<string>();
                            }
                        }

                        if (protocolFeatures["agvActions"] != null)
                        {
                            var agvActions = protocolFeatures["agvActions"] as JArray;
                            foreach (var agvAction in agvActions)
                            {
                                var actionType = agvAction["actionType"]?.ToObject<string>();
                                var actionDescription = agvAction["actionDescription"]?.ToObject<string>();
                                var actionScopes = agvAction["actionScopes"]?.ToObject<int[]>();
                            }
                        }

                        if (protocolFeatures["actionParameters"] != null)
                        {
                            var actionParameters = protocolFeatures["actionParameters"] as JArray;
                            foreach (var actionParameter in actionParameters)
                            {

                                var key = actionParameter["key"]?.ToObject<string>();
                                var valueDataType = actionParameter["valueDataType"]?.ToObject<int>();
                                var description = actionParameter["description"]?.ToObject<string>();
                                var isOptional = actionParameter["isOptional"]?.ToObject<bool>();
                            }
                        }
                        var resultDescription = protocolFeatures["resultDescription"].ToObject<string>();

                        if (agvGeometry["wheelDefinitions"] != null)
                        {
                            var wheelDefinitions = agvGeometry["wheelDefinitions"] as JArray;
                            foreach (var wheelDefinition in wheelDefinitions)
                            {
                                var type = wheelDefinition["type"]?.ToObject<int>();
                                var isActiveDriven = wheelDefinition["isActiveDriven"]?.ToObject<bool>();
                                var isActiveSteered = wheelDefinition["isActiveSteered"]?.ToObject<bool>();
                                var position = wheelDefinition["position"] as JObject;

                                var x = position["x"]?.ToObject<double>();
                                var y = position["y"]?.ToObject<double>();
                                var theta = position["theta"]?.ToObject<double>();
                                var diameter = wheelDefinition["diameter"]?.ToObject<double>();
                                var width2 = wheelDefinition["width"]?.ToObject<double>();
                                var centerDisplacement = wheelDefinition["centerDisplacement"]?.ToObject<double>();
                                var constraints = wheelDefinition["constraints"]?.ToObject<string>();
                            }
                        }

                        if (agvGeometry["envelopes2d"] != null)
                        {
                            var envelopes2ds = agvGeometry["envelopes2d"] as JArray;
                            foreach (var envelopes2d in envelopes2ds)
                            {
                                var set = envelopes2d["set"]?.ToObject<string>();
                                var polygonPoints = envelopes2d["polygonPoints"] as JArray;
                                foreach (var polygonPoint in polygonPoints)
                                {
                                    var x = polygonPoint["x"]?.ToObject<double>();
                                    var y = polygonPoint["y"]?.ToObject<double>();
                                }
                                var description = envelopes2d["description"]?.ToObject<string>();
                            }
                        }

                        if (agvGeometry["envelopes3d"] != null)
                        {
                            var envelopes3ds = agvGeometry["envelopes3d"] as JArray;
                            foreach (var envelopes3d in envelopes3ds)
                            {
                                var set = envelopes3d["set"]?.ToObject<string>();
                                var format = envelopes3d["format"]?.ToObject<string>();
                                var data2 = envelopes3d["data"] as JObject;
                                var url = envelopes3d["url"]?.ToObject<string>();
                                var description = envelopes3d["description"]?.ToObject<string>();
                            }
                        }

                        var loadPositions = loadSpecification["loadPositions"]?.ToObject<string[]>();
                        var loadSets = loadSpecification["loadSets"] as JArray;
                        foreach (var loadSet in loadSets)
                        {
                            var setName = loadSet["setName"]?.ToObject<string>();
                            var loadType = loadSet["loadType"]?.ToObject<string>();
                            var loadPositions2 = loadSpecification["loadPositions"]?.ToObject<string[]>();
                            var boundBoxReference = loadSpecification["boundBoxReference"] as JObject;
                            var loadDimensions = loadSpecification["loadDimensions"] as JObject;
                            var maxWeight = loadSet["maxWeight"]?.ToObject<double>();
                            var minLoadhandlingHeight = loadSet["minLoadhandlingHeight"]?.ToObject<double>();
                            var maxLoadhandlingHeight = loadSet["maxLoadhandlingHeight"]?.ToObject<double>();
                            var minLoadhandlingDepth = loadSet["minLoadhandlingDepth"]?.ToObject<double>();
                            var maxLoadhandlingDepth = loadSet["maxLoadhandlingDepth"]?.ToObject<double>();
                            var minLoadhandlingTilt = loadSet["minLoadhandlingTilt"]?.ToObject<double>();
                            var maxLoadhandlingTilt = loadSet["maxLoadhandlingTilt"]?.ToObject<double>();
                            var agvSpeedLimit = loadSet["agvSpeedLimit"]?.ToObject<double>();
                            var agvAccelerationLimit = loadSet["agvAccelerationLimit"]?.ToObject<double>();
                            var agvDecelerationLimit = loadSet["agvDecelerationLimit"]?.ToObject<double>();
                            var pickTime = loadSet["pickTime"]?.ToObject<double>();
                            var dropTime = loadSet["dropTime"]?.ToObject<double>();
                            var description = loadSet["description"]?.ToObject<double>();
                        }


                    }
                    catch
                    {
                    }
                }
                //
                if (topic.EndsWith("/connection"))
                {
                    try
                    {
                        var dataStr = Encoding.UTF8.GetString(data);
                        var json = JObject.Parse(dataStr);
                        //Tools.trace(dataStr);
                        var headerId = json["headerId"]?.ToObject<uint>();
                        var timestamp = json["timestamp"]?.ToObject<string>();
                        var version = json["version"]?.ToObject<string>();
                        var manufacturer = json["manufacturer"]?.ToObject<string>();
                        var serialNumber = json["serialNumber"]?.ToObject<string>();

                        var agv = API.GetAgv(serialNumber);
                        if (agv == null)
                            agv = API.Agvs.Where(x => x.GetValue<string>("serialNumber") == serialNumber).FirstOrDefault();

                        var connectionState = json["connectionState"]?.ToObject<string>();
                    }
                    catch
                    {
                    }
                }
                if (topic.EndsWith("/state"))
                {
                    //Tools.trace("接收到state");
                    try
                    {
                        var dataStr = Encoding.UTF8.GetString(data);
                        var json = JObject.Parse(dataStr);
                        //Tools.trace(dataStr);
                        var headerId = json["headerId"]?.ToObject<uint>();
                        var timestamp = json["timestamp"]?.ToObject<string>();
                        var version = json["version"]?.ToObject<string>();
                        var manufacturer = json["manufacturer"]?.ToObject<string>();
                        var serialNumber = json["serialNumber"]?.ToObject<string>();

                        var agv = API.GetAgv(serialNumber);
                        if (agv == null)
                            agv = API.Agvs.Where(x => x.GetValue<string>("serialNumber") == serialNumber).FirstOrDefault();
                        var orderId = json["orderId"]?.ToObject<string>();
                        var orderUpdateId = json["orderUpdatedId"]?.ToObject<uint>();
                        var zoneSetId = json["zoneSetId"]?.ToObject<string>();

                        var lastNodeId = json["lastNodeId"]?.ToObject<string>();
                        var lastNodeSequenceId = json["lastNodeSequenceId"]?.ToObject<uint>();

                        if (json["nodeStates"] != null)
                        {
                            var nodeStates = json["nodeStates"] as JArray;
                            foreach (var nodeState in nodeStates)
                            {
                                var nodeId = nodeState["nodeId"]?.ToObject<string>();
                                var sequenceId = nodeState["sequenceId"]?.ToObject<uint>();
                                var nodeDescription = nodeState["nodeDescription"]?.ToObject<string>();
                                var nodePosition = nodeState["nodePosition"] as JObject;
                                var released = nodeState["released"]?.ToObject<bool>();
                            }
                        }

                        if (json["edgeStates"] != null)
                        {
                            var edgeStates = json["edgeStates"] as JArray;
                            foreach (var edgeState in edgeStates)
                            {
                                var edgeId = edgeState["edgeId"]?.ToObject<string>();
                                var sequenceId = edgeState["sequenceId"]?.ToObject<uint>();
                                var edgeDescription = edgeState["edgeDescription"]?.ToObject<string>();
                                var released = edgeState["released"]?.ToObject<bool>();
                                var trajectory = edgeState["trajectory"] as JObject;
                            }
                        }

                        if (json["agvPosition"] != null)
                        {
                            var agvPosition = json["agvPosition"] as JObject;
                            var positionInitialized = agvPosition["positionInitialized"]?.ToObject<bool>();
                            var localizationScore = agvPosition["localizationScore"]?.ToObject<double>();
                            var deviationRange = agvPosition["deviationRange"]?.ToObject<double>();
                            var x = agvPosition["x"].ToObject<double>();
                            var y = agvPosition["y"].ToObject<double>();
                            var theta = agvPosition["theta"].ToObject<double>();
                            var mapId = agvPosition["mapId"]?.ToObject<string>();
                            var mapDescription = agvPosition["mapDescription"]?.ToObject<string>();
                            var angle = theta * 180 / Math.PI; // 弧度 → 角度（[-180°, 180°]）

                            if (agv != null)
                            {
                                Point location = new Point(x, y);
                                agv.API.LocationRecv(location); // (m)
                                var realAngle = (angle + 360) % 360; // 将角度映射到 [0°, 360°)
                                (agv as Agv).View.Radian = -realAngle * Math.PI / 180;
                                //(agv as Agv).View.Position = location;
                                agv.API.AngleRecv(-realAngle); // left - > right corrid
                                Tools.trace(-realAngle);
                                var lastNodeRecord = agv.GetValue<string>("lastIdRecord");
                                if (API.GetStation(lastNodeId) != null && lastNodeRecord != lastNodeId)
                                {
                                    agv.SetValue("lastIdRecord", lastNodeId);
                                    agv.API.NewMarkPointRecv(lastNodeId);
                                }
                            }
                        }

                        var velocity = json["velocity"] as JObject;
                        var vx = velocity["vx"]?.ToObject<double>();
                        var vy = velocity["vy"]?.ToObject<double>();
                        var omega = velocity["omega"]?.ToObject<double>();
                        agv.API.SpeedRecv(vx ?? 0.0);

                        var loads = json["loads"] as JArray;
                        if (loads != null)
                        {
                            foreach (var load in loads)
                            {
                                var loadId = load["loadId"]?.ToObject<string>();
                                var loadType = load["loadType"]?.ToObject<string>();
                                var loadPosition = load["loadPosition"]?.ToObject<string>();

                                var boundingBoxReference = load["boundingBoxReference"] as JObject;
                                var x = boundingBoxReference["x"]?.ToObject<double>();
                                var y = boundingBoxReference["y"]?.ToObject<double>();
                                var z = boundingBoxReference["z"]?.ToObject<double>();
                                var theta = boundingBoxReference["theta"]?.ToObject<double>();

                                var loadDimensions = load["loadDimensions"] as JObject;
                                var length = loadDimensions["length"]?.ToObject<double>();
                                var width = loadDimensions["width"]?.ToObject<double>();
                                var height = loadDimensions["height"]?.ToObject<double>();

                                var weight = load["weight"].ToObject<double>();
                            }
                        }

                        var driving = json["driving"]?.ToObject<bool>();
                        var paused = json["paused"]?.ToObject<bool>();
                        var newBaseRequest = json["newBaseRequest"]?.ToObject<bool>();
                        var distanceSinceLastNode = json["distanceSinceLastNode"]?.ToObject<double>();

                        if (json["actionStates"] != null)
                        {
                            var actionStates = json["actionStates"] as JArray;
                            foreach (var actionState in actionStates)
                            {
                                var actionId = actionState["actionId"]?.ToObject<string>();
                                var actionType = actionState["actionType"]?.ToObject<string>();
                                var actionDescription = actionState["actionDescription"]?.ToObject<string>();
                                var actionStatus = actionState["actionStatus"]?.ToObject<string>();
                                // 只处理抬车/卸车动作（pick/drop），排除 detectObject 等辅助动作
                                if (!string.IsNullOrEmpty(actionType) && actionType != "detectObject")
                                {
                                    agv.SetValue("ActionState", actionStatus);
                                }
                                var resultDescription = actionState["resultDescription"]?.ToObject<string>();
                            }
                        }

                        var batteryState = json["batteryState"] as JObject;
                        var batteryCharge = batteryState["batteryCharge"].ToObject<double>();
                        var batteryVoltage = batteryState["batteryVoltage"].ToObject<double>();
                        var batteryHealth = batteryState["batteryHealth"]?.ToObject<byte>();
                        var charging = batteryState["charging"]?.ToObject<bool>();
                        var reach = batteryState["reach"]?.ToObject<uint>();

                        if (agv != null)
                        {
                            Tools.trace("test Battery: " + batteryCharge);
                            agv.API.BatteryRecv(batteryCharge);
                            agv.API.VoltageRecv(batteryVoltage);
                            Tools.trace(batteryVoltage);
                        }

                        /// <summary>
                        /// errortype = 0x7001 = 机器人电机报警（驱动电机）  errorlevel = warning
                        /// errortype = 0x7002 = 机器人Tcp断联              errorlevel = warning
                        /// errortype = 0x7003 = 机器人Rtk未收敛            errorlevel = warning
                        /// errortype = 0x7004 = 机器人障碍物报警            errorlevel = warning
                        /// </summary>
                        var operatingMode = json["operatingMode"]?.ToObject<string>();
                        var errors = json["errors"] as JArray;

                        bool robotFrontMotorAlarm = false;
                        bool robotRearMotorAlarm = false;
                        bool robotTcpAlarm = false;
                        bool robotRtkAlarm = false;
                        bool robotObstacleAlarm = false;
                        bool robotRtkADataTimeout = false;
                        bool robotIoDataTimeout = false;
                        bool robotFrontObstacleAlarm = false;
                        bool robotRearObstacleAlarm = false;

                        if (errors != null)
                        {
                            foreach (var error in errors)
                            {
                                var errorType = error["errorType"]?.ToObject<string>();
                                Tools.trace(errorType);                                
                                //机器人tcp断联
                                if (string.Equals(errorType, "0x7000", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotTcpAlarm = true;
                                }
                                //机器人RTK数据断流
                                if (string.Equals(errorType, "0x7001", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotRtkADataTimeout = true;
                                }
                                //机器人RTK未收敛
                                if (string.Equals(errorType, "0x7002", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotRtkAlarm = true;
                                }
                                //IO盒数据断流
                                if (string.Equals(errorType, "0x7003", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotIoDataTimeout = true;
                                }
                                //机器人前障碍物报警
                                if (string.Equals(errorType, "0x7004", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotFrontObstacleAlarm = true;
                                }
                                //机器人后障碍物报警
                                if (string.Equals(errorType, "0x7005", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotRearObstacleAlarm = true;
                                }
                                //机器人前车电机报警
                                if (string.Equals(errorType, "0x7006", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotFrontMotorAlarm = true;
                                }
                                //机器人后车电机报警
                                if (string.Equals(errorType, "0x7007", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotRearMotorAlarm = true;
                                }
                                else if (string.Equals(errorType, "", StringComparison.OrdinalIgnoreCase))
                                {
                                    robotFrontMotorAlarm = false;
                                    robotRearMotorAlarm = false;
                                    robotTcpAlarm = false;
                                    robotRtkAlarm = false;
                                    robotObstacleAlarm = false;
                                    robotRtkADataTimeout = false;
                                    robotIoDataTimeout = false;
                                    robotRearObstacleAlarm = false;
                                    robotFrontObstacleAlarm = false;
                                    break;
                                }
                                var errorReferences = error["errorReferences"] as JArray;
                                if (errorReferences != null)
                                {
                                    foreach (var errorReference in errorReferences)
                                    {
                                        var errorReferenceKey = errorReference["errorReferenceKey"]?.ToObject<string>();
                                        var errorReferenceValue = errorReference["errorReferenceValue"]?.ToObject<string>();
                                    }
                                }
                                var errorDescription = error["errorDescription"]?.ToObject<string>();
                                var errorLevel = error["errorLevel"]?.ToObject<string>();
                            }
                        }

                        if (agv != null)
                        {
                            agv.SetValue("前车电机报警", robotFrontMotorAlarm);
                            agv.SetValue("后车电机报警", robotRearMotorAlarm);
                            agv.SetValue("机器人tcp断联", robotTcpAlarm);
                            agv.SetValue("RTK数据断流", robotRtkADataTimeout);
                            agv.SetValue("RTK未收敛", robotRtkAlarm);
                            agv.SetValue("IO盒数据断流", robotIoDataTimeout);
                            agv.SetValue("前车障碍物报警", robotFrontObstacleAlarm);
                            agv.SetValue("后车障碍物报警", robotRearObstacleAlarm);
                            Tools.trace("机器人tcp断联 SetValue = " + robotTcpAlarm);
                            // ========================================================
                            // 立即从 AGV 对象读取，验证 SetValue 是否真正保存成功
                            // ========================================================
                            //bool testMotorAlarm = agv.GetValue<bool>("机器人电机报警");
                            //Tools.trace("机器人电机报警 GetValue = "+ testMotorAlarm);                      	
                        }
                        else
                        {
                            Tools.trace("机器人电机报警： 没有找到对应的AGV对象！！！");
                        }

                        //                        foreach (var error in errors)
                        //                        {
                        //                            var errorType = error["errorType"]?.ToObject<string>();
                        //                            var errorReferences = error["errorReferences"] as JArray;
                        //                            foreach (var errorReference in errorReferences)
                        //                            {
                        //                                var referenceKey = errorReference["referenceKey"]?.ToObject<string>();
                        //                                var referenceValue = errorReference["referenceValue"]?.ToObject<string>();
                        //                            }
                        //                            var errorDescription = error["errorDescription"]?.ToObject<string>();
                        //                            var errorLevel = error["errorLevel"]?.ToObject<string>();
                        //                        }

                        var information = json["information"] as JArray;

                        if (information != null)
                        {
                            foreach (var info in information)
                            {
                                var infoType = info["infoType"]?.ToObject<string>();
                                var infoReferences = info["infoReferences"] as JArray;
                                foreach (var infoReference in infoReferences)
                                {
                                    var referenceKey = infoReference["referenceKey"]?.ToObject<string>();
                                    var referenceValue = infoReference["referenceValue"]?.ToObject<string>();
                                }
                                var infoDescription = info["infoDescription"]?.ToObject<string>();
                                var infoLevel = info["infoLevel"]?.ToObject<string>();
                            }


                            var safetyState = json["safetyState"] as JObject;
                            if (safetyState != null)
                            {
                                var eStop = safetyState["eStop"]?.ToObject<string>();
                                var fieldViolation = safetyState["fieldViolation"]?.ToObject<bool>();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.trace(ex.StackTrace.ToString());
                        //Json解析失败
                    }
                }
            }
           , clientId, 10, MQTTnet.Formatter.MqttProtocolVersion.V311, 0, false);
        }
    }
}     