﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.Entities;

public partial class QuizAnswer
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string Answer { get; set; }

    public int Score { get; set; }

    public virtual Quiz Quiz { get; set; }
}