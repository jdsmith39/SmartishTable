using System.Linq;
using Bunit;
using Shouldly;
using SmartishTable.Samples.Client.Pages;
using Xunit;

namespace SmartishTable.Tests.Client.Pages
{
    public class TableShould : TestBase
    {
        [Trait("Category", "Unit")]
        [Fact(DisplayName = "Verifies that the table is being populated and sorted by last name")]
        public void ShowUsersPopulatedInTableAndSortedByLastName()
        {
            // Act

            // Render the component that is under test (cut)
            var cut = RenderComponent<Table>();

            // Get an instance of the Table page
            var page = cut.Instance;

            // Tell the testing framework to wait until the list object on the page has been instantiated and populated with data.
            // This is when we can tell that new markup has been presented on the page and will allow us to find the table in the next step.
            cut.WaitForState(() => page.list != null && page.list.Count > 0);

            var firstRowFirstColumn = cut.Find("table tr:first-child td");

            // Assert
            firstRowFirstColumn.InnerHtml.ShouldBe(PeopleData.OrderBy(x => x.LastName).First().LastName);
        }
    }
}
