using System;
using System.Collections.Generic;

namespace ApplicationCore.Entity
{
    public class Class : BaseEntity
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public string Room { get; set; }
        public string Description { get; set; }
        public string InviteString { get; set; }
        public User MainTeacher { get; set; }
        public IList<ClassStudents> ClassStudents { get; set; }
    }
}