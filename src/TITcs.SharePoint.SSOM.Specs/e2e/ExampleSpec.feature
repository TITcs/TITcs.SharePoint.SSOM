Feature: ExampleSpec
	In order to manage a list
	As a registered user
	I want to be able to view the list and perform operation on it

@first
Scenario: View First Page
	Given I access the sharepoint site
	When I navigate to the Projects list
	Then I should see the list items paged
	And I press the next page button
	And I see more 30 results

@second
Scenario: Insert new item
	Given I access the sharepoint site
	When I navigate to the Projects list
	Then I press the new button e insert item data e save
	And I should see the new item