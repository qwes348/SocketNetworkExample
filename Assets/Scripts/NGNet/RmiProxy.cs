using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using Jamong.Server;

namespace NGNet
{
    public abstract class RmiProxy
    {
        public Client m_core = null;

        public RmiProxy()
        {
        }
        ~RmiProxy()
        {
        }

        public virtual void RmiSend( Int32 packetID, JsonMessage msg, JsonMessage.TargetEnum target )
        {
            if (m_core == null)
            {
                Console.WriteLine("클라이언트에 프록시를 부착해주세요. 패킷ID: " + packetID.ToString());
                return;
            }

            //m_core.RmiSend( rmiContext, packetID, msg );
            m_core.RmiSend(packetID, target, msg);
        }
    }
}
