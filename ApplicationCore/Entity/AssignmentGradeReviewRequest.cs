namespace ApplicationCore.Entity
{
    public enum ReviewRequestState
    {
        Accepted,
        Rejected,
        Waiting
    }
    public class AssignmentGradeReviewRequest: BaseEntity
    {
        public int StudentAssignmentGradeId { get; set; }
        public StudentAssignmentGrade StudentAssignmentGrade { get; set; }
        
        public int RequestedNewPoint {get;set;}
        public string Description { get; set; }

        public ReviewRequestState RequestState { get; set; } = ReviewRequestState.Waiting; 

        private void ChangeState(ReviewRequestState newState)
        {
            RequestState = newState;
        }

        public void Accept()
        {
            StudentAssignmentGrade.Point = RequestedNewPoint;
            ChangeState(ReviewRequestState.Accepted);
        }

        public void Reject()
        {
            ChangeState(ReviewRequestState.Rejected);
        }


    }
}