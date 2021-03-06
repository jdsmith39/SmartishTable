﻿using Microsoft.AspNetCore.Components;
using SmartishTable.Sorts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartishTable
{
    public partial class Sort<TItem>
    {
        public Sort()
        {
            onSortClick = OnSortClick;
        }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [CascadingParameter(Name = "SmartishTableRoot")]
        public Root<TItem> Root { get; set; }

        [Parameter]
        public System.Linq.Expressions.Expression<Func<TItem, object>> Field { get; set; }

        [Parameter]
        public bool IsDefaultSort { get; set; } = false;

        /// <summary>
        /// Default Sort Order.  Defaults to 1.  Subsequent Defaults should be incremented 2, 3, 4, etc.
        /// </summary>
        [Parameter]
        public int DefaultSorOrder { get; set; } = 1;

        [Parameter]
        public bool StartingSortDescending { get; set; } = false;

        [Parameter]
        public string Css { get; set; }

        private RenderFragment HeaderFragment;

        private string SortCss
        {
            get
            {
                if (!Root.UseSortCss) return "";

                return Root.ColumnSorts[Field.ToString()].SortOrder.HasValue ? Root.ColumnSorts[Field.ToString()].IsDescending ? Root.SortDescendingCss : Root.SortAscendingCss : Root.NoSortCss;
            }
        }

        private readonly Func<Task> onSortClick;

        private async Task OnSortClick()
        {
            Root.ColumnSorts.Set(Root.MaxNumberOfSorts, Field.ToString());
            await Root.Refresh(true);
        }

        protected override void OnInitialized()
        {
            if (Root.ColumnSorts == null)
                Root.ColumnSorts = new ColumnSortCollection<TItem>();
            Root.ColumnSorts.Add(Field.ToString(), new ColumnSortData<TItem>() { Field = Field, IsDescending = false, SortOrder = null });

            if (IsDefaultSort)
            {
                Root.ColumnSorts[Field.ToString()].SortOrder = DefaultSorOrder;
                Root.ColumnSorts[Field.ToString()].IsDescending = StartingSortDescending;
            }

            if (!string.IsNullOrWhiteSpace(Root.HeaderTag))
            {
                HeaderFragment = (builder) =>
                {
                    builder.OpenElement(0, Root.HeaderTag);
                    builder.AddAttribute(1, "class", $"{Css} {SortCss}");
                    builder.AddAttribute(2, "onclick", onSortClick);
                    builder.AddContent(3, ChildContent);
                    builder.CloseElement();
                };
            }
        }
    }
}
