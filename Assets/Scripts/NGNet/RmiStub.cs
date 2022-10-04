using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGears.Server;

namespace NGNet
{
    public abstract class RmiStub
    {
        public RmiStub()
        {
        }
        ~RmiStub()
        {
        }

        public abstract bool ProcessReceivedMessage( Message msg );

        public abstract bool ProcessReceivedMessage(JsonMessage msg);
    }
}
