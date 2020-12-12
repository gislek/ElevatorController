using ElevatorController.Services;
using System;

namespace ElevatorController
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var eController = new EController(3, 5);
            _ = eController.Start();

            //eController.GoToFloor(5);
            //eController.GoToFloor(3);
            //eController.GoToFloor(8);
            //eController.GoToFloor(9);
            //eController.GoToFloor(2);

            while (true)
            {
                string input = Console.ReadLine();

                if (string.Equals(input, "nodstopp", StringComparison.OrdinalIgnoreCase))
                    eController.EmergencyStop();
                else if (int.TryParse(input, out int floorNo))
                    eController.GoToFloor(floorNo);
                else if (string.Equals(input, "stopp", StringComparison.OrdinalIgnoreCase))
                    break;
            }

        }
    }
}
