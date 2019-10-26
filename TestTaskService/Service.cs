using System.ServiceProcess;
using System.Threading;

namespace TestTaskService
{
    public partial class Service : ServiceBase
    {
        TestTask testTask;

        public Service()
        {
            InitializeComponent();
            testTask = new TestTask();
        }

        protected override void OnStart(string[] args)
        {
            Thread thread = new Thread(testTask.Start);
            thread.Start();
        }

        protected override void OnStop()
        {
            testTask.Stop();
        }
    }
}
