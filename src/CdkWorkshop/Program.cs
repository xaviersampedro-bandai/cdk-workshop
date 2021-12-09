using Amazon.CDK;

namespace CdkWorkshop
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            _ = new CdkWorkshopStack(app, "CdkWorkshopStack");

            app.Synth();
        }
    }
}
