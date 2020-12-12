namespace ElevatorController.Models
{
    public class Elevator
    {
        public bool Running { get; set; }
        public int TargetFloor { get; set; }
        public int CurrentFloor { get; set; }
        public Direction Direction { get; set; }
    }
}
