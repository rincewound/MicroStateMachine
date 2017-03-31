using libSSM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libSSMTests
{
    [TestClass]
    public class SSMParseProtocol
    {
        class PayloadReceiver: IState<char>
        {
            public int ReceiveSize { get; set; }

            int receiveIndex;

            public void OnEnter(char obj)
            {
                receiveIndex++;
            }

            public void OnLeave(char obj)
            {
                //throw new NotImplementedException();
            }

            internal bool doneReceiving()
            {
                return receiveIndex >= ReceiveSize;
            }
        }

        class ReceiveLength : IState<char>
        {
            byte[] length = new byte[2];
            int lenIndex = 0;
            private PayloadReceiver receiver;

            public ReceiveLength(PayloadReceiver receiver)
            {
                this.receiver = receiver;
            }

            public void OnEnter(char obj)
            {
                length[lenIndex] = (byte)obj;
                lenIndex++;

                if (lenIndex == 2)
                    receiver.ReceiveSize = (length[0] << 8) + length[1];
            }

            public bool doneReceiving()
            {
                return lenIndex >= 2;
            }

            public void OnLeave(char obj)
            {
               
            }
        }

        StateMachine<char> sm;
        BasicState<char> idle;
        BasicState<char> stxReceived;
        BasicState<char> etxReceived;
        PayloadReceiver receivePayload;
        ReceiveLength recvLength;

        [TestInitialize]
        public void Setup()
        {
            // Parse a Frame starting with STX, 2 Byte Len, Len Bytes, ETX
            idle = new BasicState<char>("idle");
            stxReceived = new BasicState<char>("stx received");
            etxReceived = new BasicState<char>("stx received");
            receivePayload = new PayloadReceiver();
            recvLength = new ReceiveLength(receivePayload);

            var transitions = new StateTransition<char>[]
            {
                // Transitions for idle
                new StateTransition<char> {from = idle, to = stxReceived, evtFunc= x => x == 0x02 },
                new StateTransition<char> {from = idle, to = idle, evtFunc= x => true },

                // Transitions for stxReceived
                new StateTransition<char> {from = stxReceived, to = recvLength, evtFunc = x => true },

                // Transitions for recvLength
                // Note, that recvLength will trigger a statechange itself, after it received two bytes
                new StateTransition<char> {from = recvLength, to = recvLength,      evtFunc = x => true, guard = () => !recvLength.doneReceiving() },        // Guardfunc will yield true, of all bytes were received.
                new StateTransition<char> {from = recvLength, to = receivePayload,  evtFunc = x => true, guard = () =>  recvLength.doneReceiving() },        // Guardfunc will yield true, of all bytes were received.

                // Transitions for receivePayload
                new StateTransition<char> {from = receivePayload, to = receivePayload, evtFunc = x => true,   guard = () => !receivePayload.doneReceiving() },
                new StateTransition<char> {from = receivePayload, to = etxReceived, evtFunc = x => x == 0x03, guard = () =>  receivePayload.doneReceiving() },
                //new TTransition<char> {from = receivePayload, to = etxReceived, evtFunc = x => true, guard = () => receivePayload.doneReceiving() },      // Bad state, where we should receive ETX, but received s.th. else instead.

                new StateTransition<char> {from = etxReceived, to = idle, evtFunc = x => true },
            };

            sm = new StateMachine<char>(transitions, idle);
        }

        [TestMethod]
        public void ParseToSTX()
        {
            Assert.AreEqual(sm.ActiveState, idle);
            sm.Event((char) 0x02);
            Assert.AreEqual(sm.ActiveState, stxReceived);
        }

        [TestMethod]
        public void ParseLen()
        {
            Assert.AreEqual(sm.ActiveState, idle);
            sm.Event((char) 0x02);
            sm.Event((char) 0x01);
            sm.Event((char) 0xFF);
            Assert.IsTrue(recvLength.doneReceiving());
            sm.Event((char)0x01);
            Assert.AreEqual(receivePayload.ReceiveSize, (1 << 8) + 255);
        }

        [TestMethod]
        public void ParsePayload()
        {
            Assert.AreEqual(sm.ActiveState, idle);
            sm.Event((char)0x02);
            sm.Event((char)0x00);
            sm.Event((char)0x03);

            sm.Event('A');
            sm.Event('B');
            sm.Event('C');

            Assert.IsTrue(receivePayload.doneReceiving());
        }

        [TestMethod]
        public void ParseToEnd()
        {
            Assert.AreEqual(sm.ActiveState, idle);
            sm.Event((char)0x02);
            sm.Event((char)0x00);
            sm.Event((char)0x03);

            sm.Event('A');
            sm.Event('B');
            sm.Event('C');

            sm.Event((char)0x03);

            Assert.AreEqual(sm.ActiveState, etxReceived);
        }


    }
}
