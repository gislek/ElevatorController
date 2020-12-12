using ElevatorController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElevatorController.Services
{
    public class EController
    {
        private readonly Elevator _elevator;
        private readonly int _secondsPerFloor;
        private readonly int _secondsPerStop;
        private readonly List<int> _queue;
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool emergencyStop = false;

        public EController(int secondsPerFloor, int secondsPerStop)
        {
            _cancellationToken = new CancellationToken();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            _secondsPerFloor = secondsPerFloor;
            _secondsPerStop = secondsPerStop;
            _elevator = new Elevator();
            _queue = new List<int>();
        }

        public async Task Start()
        {
            Console.WriteLine("Heissystem aktivert");
            while(!emergencyStop)
            {
                if (_elevator.Running)
                    return;

                if (_queue.Any())
                {
                    _elevator.Running = true;
                    await Run();
                    Console.WriteLine($"Venter");
                    _elevator.Running = false;
                }
                await Task.Delay(500);
            }
        }

        private async Task Run()
        {
            while (_queue.Any() && !emergencyStop)
            {
                _elevator.TargetFloor = _queue.FirstOrDefault();
                Console.WriteLine($"Går til {_elevator.TargetFloor} etasje");

                _elevator.Direction = _elevator.TargetFloor > _elevator.CurrentFloor ? Direction.Up : Direction.Down;
                int floorsToMove = Math.Abs(_elevator.TargetFloor - _elevator.CurrentFloor);

                for (int i = 0; i < floorsToMove; i++)
                {
                    // Stop for time between floors
                    await Task.Delay(_secondsPerFloor * 1000, _cancellationToken);

                    if (emergencyStop)
                        return;

                    _elevator.CurrentFloor += (int)_elevator.Direction;
                    Console.WriteLine($"Ankommet {_elevator.CurrentFloor} etasje");

                    if (_queue.Contains(_elevator.CurrentFloor))
                    {
                        // Stopping for time on floor
                        Console.WriteLine("Stopper");
                        await Task.Delay(_secondsPerStop * 1000, _cancellationToken);
                        if (emergencyStop)
                            return;

                        _queue.Remove(_elevator.CurrentFloor);
                    }
                }
            }
        }

        public Direction GetDirection() => _elevator.Direction;

        public void EmergencyStop()
        {
            Console.WriteLine($"Nødstopp aktivert i {_elevator.CurrentFloor} etasje");
            
            emergencyStop = true;
            _cancellationTokenSource.Cancel();
        }

        public void GoToFloor(int floorNo)
        {
            if (floorNo == _elevator.CurrentFloor)
                return;

            var time = TimeToFloor(floorNo);

            if (!_queue.Contains(floorNo))
            {
                _queue.Add(floorNo);
                Console.WriteLine($"{floorNo} etasje lagt i køen, forventet ankomst om {time.Minutes}m{time.Seconds}s");
            }
            else
            {
                Console.WriteLine($"{floorNo} etasje er allerede i køen, forventet ankomst om {time.Minutes}m{time.Seconds}s");
            }
        }

        public TimeSpan TimeToFloor(int floorNo)
        {
            int timeUsed = 0;

            var queue = new List<int>(_queue);

            if (!queue.Contains(floorNo))
                queue.Add(floorNo);

            var currentFloor = _elevator.CurrentFloor;
            var currentTarget = _elevator.TargetFloor;

            while (currentFloor != floorNo)
            {
                int floorsToMove = Math.Abs(currentTarget - currentFloor);
                bool goingUp = currentTarget > currentFloor;

                for (int i = 0; i < floorsToMove; i++)
                {
                    currentFloor += goingUp ? 1 : -1;

                    // Moved one floor, add time used
                    timeUsed += _secondsPerFloor;


                    if (currentFloor == floorNo)
                        break;

                    if (queue.Contains(currentFloor))
                    {
                        // Stopping on this floor, add time used
                        timeUsed += _secondsPerStop;
                        queue.Remove(currentFloor);
                    }
                }

                // Target floor reached, get next target
                if (queue.Count > 0)
                    currentTarget = queue.FirstOrDefault();
            }

            return TimeSpan.FromSeconds(timeUsed);
        }
    }
}
