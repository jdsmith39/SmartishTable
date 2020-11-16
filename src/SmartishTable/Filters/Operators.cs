using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartishTable.Filters
{
    public enum StringOperators : byte
    {
        [Display(Name = "Contains")]
        Contains = 0,
        [Display(Name = "Starts With")]
        StartsWith = 1,
        [Display(Name = "Ends With")]
        EndsWith = 2,
        /// <summary>
        /// ==
        /// </summary>
        [Display(Name = "Equals", ShortName = "==")]
        Equals = 3,
        /// <summary>
        /// !=
        /// </summary>
        [Display(Name = "Not Equals", ShortName = "!=")]
        NotEquals = 4,
    }

    public enum NumericOperators : byte
    {
        /// <summary>
        /// ==
        /// </summary>
        [Display(Name = "Equals", ShortName = "==")]
        Equals = 3,
        /// <summary>
        /// !=
        /// </summary>
        [Display(Name = "Not Equals", ShortName = "!=")]
        NotEquals = 4,
        /// <summary>
        /// >
        /// </summary>
        [Display(Name = "Greater Than", ShortName = ">")]
        GreaterThan = 5,
        /// <summary>
        /// >=
        /// </summary>
        [Display(Name = "Greater Than Or Equal", ShortName = ">=")]
        GreaterThanOrEqual = 6,
        /// <summary>
        /// &lt
        /// </summary>
        [Display(Name = "Less Than", ShortName = "<")]
        LessThan = 7,
        /// <summary>
        /// &lt=
        /// </summary>
        [Display(Name = "Less Than Or Equal", ShortName = "<=")]
        LessThanOrEqual = 8,
    }

    public enum DateTimeOperators : byte
    {
        /// <summary>
        /// ==
        /// </summary>
        [Display(Name = "Equals", ShortName = "==")]
        Equals = 3,
        /// <summary>
        /// !=
        /// </summary>
        [Display(Name = "Not Equals", ShortName = "!=")]
        NotEquals = 4,
        /// <summary>
        /// >
        /// </summary>
        [Display(Name = "Greater Than", ShortName = ">")]
        GreaterThan = 5,
        /// <summary>
        /// >=
        /// </summary>
        [Display(Name = "Greater Than Or Equal", ShortName = ">=")]
        GreaterThanOrEqual = 6,
        /// <summary>
        /// &lt
        /// </summary>
        [Display(Name = "Less Than", ShortName = "<")]
        LessThan = 7,
        /// <summary>
        /// &lt=
        /// </summary>
        [Display(Name = "Less Than Or Equal", ShortName = "<=")]
        LessThanOrEqual = 8,
    }

    public enum BooleanOperators : byte
    {
        /// <summary>
        /// ==
        /// </summary>
        [Display(Name = "Equals", ShortName = "==")]
        Equals = 3,
        /// <summary>
        /// !=
        /// </summary>
        [Display(Name = "Not Equals", ShortName = "!=")]
        NotEquals = 4,
        [Display(Name = "Is True")]
        IsTrue = 9,
        [Display(Name = "Is False")]
        IsFalse = 10,
    }
}
