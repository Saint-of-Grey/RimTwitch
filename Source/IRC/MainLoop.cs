using System.Threading;

namespace RimTwitch.IRC
{
    public class Loop
    {
        protected IrcClient _irc;
        protected Thread _thread;
        protected bool Stopped;

        // Empty constructor makes instance of Thread
        public Loop(IrcClient irc)
        {
            _irc = irc;
            _thread = new Thread(new ThreadStart(this.Run));
        }

        // Starts the thread
        public void Start()
        {
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected virtual void Run()
        {
            while (!Stopped)
            {
                Broadcast.Tick();
                Thread.Sleep(1);
            }
        }

        public void Stop()
        {
            Stopped = true;
            _thread.Interrupt();
        }
    }
}