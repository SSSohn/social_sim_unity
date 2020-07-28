/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */

using Newtonsoft.Json;

using RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient.MessageTypes.Pedsim
{
    public class SocialActivities : Message
    {
        [JsonIgnore]
        public const string RosMessageName = "pedsim_msgs/SocialActivities";

        public Header header;
        //  All social activities that have been detected in the current time step,
        //  within sensor range of the robot.
        public SocialActivity[] elements;

        public SocialActivities()
        {
            this.header = new Header();
            this.elements = new SocialActivity[0];
        }

        public SocialActivities(Header header, SocialActivity[] elements)
        {
            this.header = header;
            this.elements = elements;
        }
    }
}