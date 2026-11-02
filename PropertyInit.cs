namespace ProfControl
{
    public partial class VDA5050 : IAppScript
    {
        public void PropertyInit()
        {
            var property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("VDA5050");
            var detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.Title);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("MQTT账户名称");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);
            property.SavePropertyAdd(SerializeModeEnum.All);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("MQTT密码");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);
            property.SavePropertyAdd(SerializeModeEnum.All);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("manufacturer");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);
            property.SavePropertyAdd(SerializeModeEnum.All);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("serialNumber");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);
            property.SavePropertyAdd(SerializeModeEnum.All);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("version");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);
            property.SavePropertyAdd(SerializeModeEnum.All);

            var propertyDouble = GlobalPropertyHelper.GPropertyFactory<Agv, double>("allowedDeviationXY");
            propertyDouble.PropertyAdd(WorkSystemModeEnum.EditMode);
            propertyDouble.SavePropertyAdd(SerializeModeEnum.All);

            propertyDouble = GlobalPropertyHelper.GPropertyFactory<Agv, double>("allowedDeviationTheta");
            propertyDouble.PropertyAdd(WorkSystemModeEnum.EditMode);
            propertyDouble.SavePropertyAdd(SerializeModeEnum.All);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("mapId");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);
            property.SavePropertyAdd(SerializeModeEnum.All);

            property = GlobalPropertyHelper.GPropertyFactory<Agv, string>("lastIdRecord");
            detail = property.PropertyAdd(WorkSystemModeEnum.EditMode);
            detail.SetControlType(PropertyCtrlEnum.TextBox);

            var propertyBool = GlobalPropertyHelper.GPropertyFactory<Agv, bool>("eventAdded");
            detail = propertyBool.PropertyAdd(WorkSystemModeEnum.EditMode);
            /// <summary>
            /// 新增机器人电机报警属性
            /// </summary>            
//            propertyBool = GlobalPropertyHelper.GPropertyFactory<Agv, bool>("MotorAlarm");
//            detail = propertyBool.PropertyAdd(WorkSystemModeEnum.EditMode);
//            detail.SetControlType(PropertyCtrlEnum.TextBox);
//            propertyBool.SavePropertyAdd(SerializeModeEnum.All);
            //var motorAlarmDetail = motorAlarmProperty.PropertyAdd(WorkSystemModeEnum.EditMode);
            //motorAlarmDetail.SetControlType(PropertyCtrlEnum.CheckBox);

        }

    }

}                  