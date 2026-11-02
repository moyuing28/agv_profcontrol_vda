using ProfControl.Modules.Information;
using System.Windows.Media;
namespace ProfControl
{
    public partial class VDA5050 : IAppScript
    {
        string clientId = "ProfControl";
        System.UInt32 taskId = 1;
        System.UInt32 instantactionId = 1;
        System.UInt32 actionId = 1;

        public void Main()
        {
            while (true)
            {
                if (Tools.IsOpenedProject)
                    break;
                Thread.Sleep(100);
            }

            //设备监视注册（一次性）：在设备监视列表显示机器人TCP连接状态（数据源：RecvData 解析的 tcpBridge）             
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "机器人tcp断联", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("机器人tcp断联");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示机器人电机是否报警
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "前车电机报警", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("前车电机报警");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示机器人电机是否报警
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "后车电机报警", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("后车电机报警");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示机器人前车是否遇到障碍物
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "前车障碍物报警", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("前车障碍物报警");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示机器人后车是否遇到障碍物
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "后车障碍物报警", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("后车障碍物报警");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示RTK是否收敛
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "RTK未收敛", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("RTK未收敛");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示RTK是否断流
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "RTK数据断流", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("RTK数据断流");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });
            //设备监测注册（一次性）：在设备监测列表显示IO盒数据是否断流
            API.增加设备监视内容<IAgv, bool>("Agv设备监视", "IO盒数据断流", API.设备监视组件枚举.指示灯, API.放置区域.尾部, (IMonitorItem item, IAgv agv) =>
            {
                bool isAlarm = agv.GetValue<bool>("IO盒数据断流");
                // ★ 关键：显式控制颜色，true → 红灯，false → 透明（不亮）
                item.Color = isAlarm ? Brushes.Red : Brushes.Transparent;
                return isAlarm;
            });          


            /// mqtt主题订阅
            MqttInit();
            //事件注册
            EventAdd();
            while (true)
            {

                Thread.Sleep(500);
            }
        }

        public void Kill()
        {
            ///事件注销
            EventKill();
        }

    }
}               