@Safespots
Feature: Safespots
In order to intelligently flee to safety
As the bot
I want to determine if I should flee and where to go

Background:
Given Items exist
| !this     | TypeID | Type                        | GroupID | Group           |
| <<cloak>> | 11370  | Prototype Cloaking Device I | 330     | Cloaking Device |

Scenario: Am I safe - In Station
Given I am in a station
When I check if I am safe
Then the result should be 'true'

Scenario: Am I safe - In space, within tower shields of any tower
Given I am in space
And I have entities
# True Sansha Control Tower - ShieldRadius = 30,000m; Control Tower GroupID = 365
| Name             | X | Y | Z | TypeID | Distance | GroupID |
| Cylons Were Here | 1 | 1 | 1 | 27786  | 29999    | 365     |
And I have item info
| TypeID | Type                      | ShieldRadius |
| 27786  | True Sansha Control Tower | 30000        |
When I check if I am safe
Then the result should be 'true'

Scenario: Am I safe - In Space, within warp range of a "safe" bookmark, local is safe
Given I am in space
And I am in solarsystem ID '1'
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
| safe: bookmark | 0 | 0 | 0 | 1             |
# some point within 150km of the above
And I am at point '0','0','0'
And local is safe
When I check if I am safe
Then the result should be 'true'

Scenario: Am I safe - In Space, within warp range of a "safe" bookmark, local is not safe but I have a cloak and can cloak
Given I am in space
And I am in solarsystem ID '1'
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
| safe: bookmark | 0 | 0 | 0 | 1             |
# some point within 150km of the above
And I am at point '0','0','0'
And local is not safe
And I have cloaking device modules
| ID       | ToItem    |
| 00000001 | <<cloak>> |
And I have entities
| ID | IsTargetingMe |
When I check if I am safe
Then the result should be 'true'

Scenario: Am I safe - In Space, within warp range of a "safe" bookmark, local is not safe but I have a cloak and can't cloak
Given I am in space
And I am in solarsystem ID '1'
And my safe bookmark prefix is 'safe: '
And I have entities
| ID | Name        | X | Y | Z | GroupID | Distance     | IsTargetingMe |
| 1  | Joe Jackass | 0 | 0 | 0 | 444     | 150000       | true          |
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
| safe: bookmark | 0 | 0 | 0 | 1             |
# some point within 150km of the above
And I am at point '0','0','0'
And local is not safe
And I have cloaking device modules
| ID       | ToItem    |
| 00000001 | <<cloak>> |
When I check if I am safe
Then the result should be 'false'

Scenario: Am I safe - In Space, within warp range of a "safe" bookmark, bookmark in different system
Given I am in space
And I am in solarsystem ID '0'
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
| safe: bookmark | 0 | 0 | 0 | 1             |
# some point within 150km of the above
And I am at point '0','0','0'
When I check if I am safe
Then the result should be 'false'

Scenario: Am I safe - In space, no "safe" bookmarks, within warp-in range of a planet, local is safe
Given I am in space
# Note: We specify the distance to an entity because EVE provides Distance as a computed field
And I have entities
| ID | Name       | X | Y | Z | GroupID | Distance     |
| 1  | Dodixie IX | 0 | 0 | 0 | 7       | 100000000000 |
# No safe bookmarks
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
And local is safe
When I check if I am safe
Then the result should be 'true'

Scenario: Am I safe - In space, no "safe" bookmarks, within warp-in range of a planet, local is not safe but I have a cloak and can cloak
Given I am in space
# Note: We specify the distance to an entity because EVE provides Distance as a computed field
And I have entities
| ID | Name        | X | Y | Z | GroupID | Distance     | IsTargetingMe |
| 1  | Dodixie IX  | 0 | 0 | 0 | 7       | 100000000000 | false         |
# No safe bookmarks
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
And local is not safe
And I have cloaking device modules
| ID       | ToItem    |
| 00000001 | <<cloak>> |
When I check if I am safe
Then the result should be 'true'

Scenario: Am I safe - In space, no "safe" bookmarks, within warp-in range of a planet, local is not safe but I have a cloak and can't cloak
Given I am in space
# Note: We specify the distance to an entity because EVE provides Distance as a computed field
And I have entities
| ID | Name        | X | Y | Z | GroupID | Distance     | IsTargetingMe |
| 1  | Dodixie IX  | 0 | 0 | 0 | 7       | 100000000000 | false         |
| 2  | Joe Jackass | 0 | 0 | 0 | 444     | 150000       | true          |
# No safe bookmarks
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
And local is not safe
And I have cloaking device modules
| ID       | ToItem    |
| 00000001 | <<cloak>> |
When I check if I am safe
Then the result should be 'false'

Scenario: Am I safe - Negative test - If we're not explicitly at a safespot, we shouldn't be considered at one
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X | Y | Z | SolarSystemID |
When I check if I am safe
Then the result should be 'false'

Scenario: Am I safe - In space, safe bookmarks, within warp-in range of a planet
Given I am in space
# Note: We specify the distance to an entity because EVE provides Distance as a computed field
And I have entities
| ID | Name        | X | Y | Z | GroupID | Distance     | IsTargetingMe |
| 1  | Dodixie IX  | 0 | 0 | 0 | 7       | 100000000000 | false         |
# No safe bookmarks
And I am at point '0','0','0'
And I am in solarsystem ID '1'
And my safe bookmark prefix is 'safe: '
And local is safe
And I have bookmarks
| Label          | X      | Y         | Z    | SolarSystemID |
| safe: bookmark | 146546 | -64343684 | 4646 | 1             |
When I check if I am safe
Then the result should be 'false'

Scenario: Am I safe - In space, no safe bookmarks, stations present, within warp-in range of a planet
Given I am in space
# Note: We specify the distance to an entity because EVE provides Distance as a computed field
And I have entities
| ID | Name                                         | X   | Y   | Z   | GroupID | Distance     | IsTargetingMe |
| 1  | Dodixie IX                                   | 0   | 0   | 0   | 7       | 100000000000 | false         |
| 2  | Dodixie IX - 9 - Astral Mining Inc. Refinery | 100 | 100 | 100 | 15      | 750987       | false         |
# No safe bookmarks
And I am at point '0','0','0'
And I am in solarsystem ID '1'
And my safe bookmark prefix is 'safe: '
And local is safe
And I have bookmarks
| Label          | X      | Y         | Z    | SolarSystemID |
When I check if I am safe
Then the result should be 'false'

Scenario: Get safespot - All options available, should pick space safe
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label          | X        | Y        | Z          | SolarSystemID | ItemID | ID |
| safe: bookmark | 16465    | 79798498 | -47487676  | 1             | -1     | 1  |
| safe: station  | -6546546 | -2       | 1316354685 | 1             | 2      | 2  |
And I am in solarsystem ID '1'
And I have entities
| ID | Name                                         | X   | Y   | Z   | GroupID | Distance     | IsTargetingMe |
| 1  | Dodixie IX                                   | 0   | 0   | 0   | 7       | 900000000000 | false         |
| 2  | Dodixie IX - 9 - Astral Mining Inc. Refinery | 100 | 100 | 100 | 15      | 750987       | false         |
And my home station is 'Dodixie IX - 9 - Astral Mining Inc. Refinery'
When I get a safe spot
Then the the destination should be a bookmark destination matching bookmark ID '1'

Scenario: Get safespot - No space safes available, should pick station safe
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label         | X        | Y  | Z          | SolarSystemID | ItemID | ID |
| safe: station | -6546546 | -2 | 1316354685 | 1             | 200    | 2  |
And I am in solarsystem ID '1'
And I have entities
| ID  | Name                                         | X   | Y   | Z   | GroupID | Distance     | IsTargetingMe |
| 1   | Dodixie IX                                   | 0   | 0   | 0   | 7       | 900000000000 | false         |
| 200 | Dodixie IX - 9 - Astral Mining Inc. Refinery | 100 | 100 | 100 | 15      | 750987       | false         |
And my home station is 'Dodixie IX - 9 - Astral Mining Inc. Refinery'
When I get a safe spot
Then the the destination should be an entity destination matching entity ID '200'

Scenario: Get safespot - No station safes available, should pick home station
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label         | X        | Y  | Z          | SolarSystemID | ItemID | ID |
And I have entities
| ID  | Name                                         | X   | Y   | Z   | GroupID | Distance     | IsTargetingMe |
| 1   | Dodixie IX                                   | 0   | 0   | 0   | 7       | 900000000000 | false         |
| 200 | Dodixie IX - 9 - Astral Mining Inc. Refinery | 100 | 100 | 100 | 15      | 750987       | false         |
And my home station is 'Dodixie IX - 9 - Astral Mining Inc. Refinery'
When I get a safe spot
Then the the destination should be an entity destination matching entity ID '200'
And we should dock at the destination
And the destination's distance should be '200'

Scenario: Get safespot - No home station available, should pick any station
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label         | X        | Y  | Z          | SolarSystemID | ItemID | ID |
And I have entities
| ID  | Name                                         | X   | Y   | Z   | GroupID | Distance     | IsTargetingMe |
| 1   | Dodixie IX                                   | 0   | 0   | 0   | 7       | 900000000000 | false         |
| 200 | Dodixie IX - 9 - Astral Mining Inc. Refinery | 100 | 100 | 100 | 15      | 750987       | false         |
And my home station is 'Dodixie IV - 20 - Blaze It Fgts'
When I get a safe spot
Then the the destination should be an entity destination matching entity ID '200'
And we should dock at the destination
And the destination's distance should be '200'

Scenario: Get safespot - No station available, should pick a planet
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label         | X        | Y  | Z          | SolarSystemID | ItemID | ID |
And I have entities
| ID  | Name       | X | Y | Z | GroupID | Distance     | IsTargetingMe |
| 100 | Dodixie IX | 0 | 0 | 0 | 7       | 900000000000 | false         |
When I get a safe spot
Then the the destination should be an entity destination matching entity ID '100'
And the destination's distance should be '100000000000'

Scenario: Get safespot - Should not return any safespot we're already at - Space safe
Given I am in space
And I am at point '16465','79798498','-47487676'
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label            | X        | Y        | Z          | SolarSystemID | ItemID | ID |
| safe: bookmark   | 16465    | 79798498 | -47487676  | 1             | -1     | 1  |
| safe: bookmark 2 | -6546546 | -2       | 1316354685 | 1             | -1     | 2  |
And I am in solarsystem ID '1'
And I have entities
| ID | Name                                         | X   | Y   | Z   | GroupID | Distance     | IsTargetingMe |
When I get a safe spot
Then the the destination should be a bookmark destination matching bookmark ID '2'

Scenario: Get safespot - Should not return any safespot we're already at - Planets
Given I am in space
And my safe bookmark prefix is 'safe: '
And I have bookmarks
| Label            | X        | Y        | Z          | SolarSystemID | ItemID | ID |
And I am in solarsystem ID '1'
And I have entities
| ID  | Name       | GroupID | Distance        |
| 100 | Dodixie IX | 7       | 40000000000     |
| 101 | Dodixie X  | 7       | 871049581709827 |
When I get a safe spot
Then the the destination should be an entity destination matching entity ID '101'