@DropOffToJetissonContainerActionTest
Feature: DropOffToJetissonContainerActionTest
	As a miner
	I want to unload ore to a jetisson container

Scenario: 1 a - When no jetcan exists / is active, and I have ore in my ore hold, create a new one
Given I have ore hold items
| !this      | Name     |
| <scordite> | Scordite |
And I have entities
| !this            | ID       | Name                | IsLockedTarget |
When I process jetisson container dropoff ('1' time(s))
Then item '<scordite>' should have been jetissoned

Scenario: 1 b - When no jetcan exists / is active, and I have ore in my cargo hold, create a new one
Given I have cargo hold items
| !this      | Name     | CategoryID |
| <scordite> | Scordite | 25         |
And I have entities
| !this            | ID       | Name                | IsLockedTarget |
When I process jetisson container dropoff ('1' time(s))
Then item '<scordite>' should have been jetissoned

Scenario: 1 c - When no jetcan exists / is active, and I have ore in my ore hold and cargo, create a new one from the ore hold item
Given I have cargo hold items
| !this      | Name     | CategoryID |
| <scordite> | Scordite | 25         |
And I have ore hold items
| !this      | Name     |
| <veldspar> | Veldspar |
And I have entities
| !this            | ID       | Name                | IsLockedTarget |
When I process jetisson container dropoff ('1' time(s))
Then item '<veldspar>' should have been jetissoned
And item '<scordite>' should not have been jettisoned

Scenario: 2 a - When no jetcan was active, and I had no existing cans, and I jettison an item, I should detect the new can and make it active

Scenario: 2 b - When no jetcan was active, and I had existing nearby jetcans, and I jettison an item, I should detect the new can and make it active

Scenario: 3 - Only try to create a new jetisson container after the 3-minute cooldown

Scenario: 4 - When the active jetcan is nearly full, it should be considered full and marked for pickup

Scenario: 5 - When no jetcan exists / is active, and I create one from either source, and jetisson a full can, it should finish the dropoff cycle renamed and pickup requested