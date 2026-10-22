using System;
using System.Collections.Generic;

namespace Practicals.Models.Day2DB;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? StudentId { get; set; }

    public virtual Student? Student { get; set; }
}
