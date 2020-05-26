@Targeting
Feature: Targeting

#@mytag

Scenario: Unlock unqueued targets 1 - Multiple targets to unlock
Given I have entities
| !this            | ID       | Name                | IsLockedTarget |
| <scorditeEntity> | 00000001 | Asteroid (Scordite) | true           |
| <veldsparEntity> | 00000002 | Asteroid (Veldspar) | true           |
And I have queued targets
| !this    | Id       | Type   |
When I unlock unqueued targets
Then exactly '1' locked target(s) should be unlocked
And Unlock unqueued targets should return 'true'

Scenario: Unlock unqueued targets 2 - No targets to unlock
Given I have entities
| !this            | ID       | Name                | IsLockedTarget |
And I have queued targets
| !this    | Id       | Type   |
When I unlock unqueued targets
Then exactly '0' locked target(s) should be unlocked
And Unlock unqueued targets should return 'false'

Scenario: Unlock unqueued targets 3 - Queued targets shouldn't be unlocked
Given I have queued targets
| !this    | Id       | Type |
| <target> | 00000001 | Mine |
And I have entities
| !this          | ID       | Name                | IsLockedTarget |
| <targetEntity> | 00000001 | Asteroid (Scordite) | true           |
When I unlock unqueued targets
Then exactly '0' locked target(s) should be unlocked
And Unlock unqueued targets should return 'false'

Scenario: Lock targets 1 - 1 entity, 1 queued, 1 type, max 1
Given I have queued targets
| !this    | Id       | Type |
| <target> | 00000001 | Mine |
And I have entities
| !this          | ID       | Name                | IsLockedTarget |
| <targetEntity> | 00000001 | Asteroid (Scordite) | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
And my ship can lock '1' targets
# 2 pulses (1 target + buffer for unlock)
When I process queued targets '2' times
Then target '<targetEntity>' should have been locked
And exactly '0' locked target(s) should be unlocked

Scenario: Lock targets 2 - 2 entity, 2 queued, 1 type, max 2
Given I have queued targets
| !this     | Id       | Type |
| <target1> | 00000001 | Mine |
| <target2> | 00000002 | Mine |
And I have entities
| !this           | ID       | Name                | IsLockedTarget |
| <targetEntity1> | 00000001 | Asteroid (Scordite) | false          |
| <targetEntity2> | 00000002 | Asteroid (Veldspar) | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
And my ship can lock '2' targets
# 3 pulses (2 targets + unlock buffer)
When I process queued targets '3' times
Then target '<targetEntity1>' should have been locked
And target '<targetEntity2>' should have been locked
And exactly '0' locked target(s) should be unlocked

Scenario: Lock targets 3 - 2 entity, 2 queued, 2 type, max 2
Given I have queued targets
| !this     | Id       | Type |
| <target1> | 00000001 | Mine |
| <target2> | 00000002 | Kill |
And I have entities
| !this           | ID       | Name                | IsLockedTarget |
| <targetEntity1> | 00000001 | Asteroid (Scordite) | false          |
| <targetEntity2> | 00000002 | Serpentis Spy       | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
And my ship can lock '2' targets
# 3 pulses (2 targets + unlock buffer)
When I process queued targets '3' times
Then target '<targetEntity1>' should have been locked
And target '<targetEntity2>' should have been locked
And exactly '0' locked target(s) should be unlocked

Scenario: Lock targets 3 - 4 entity, 4 queued, 2 type, max 3 - Should use more combat slots in a combat mode
Given I have queued targets
| !this     | Id       | Type | Priority |
| <target1> | 00000001 | Mine | 1        |
| <target2> | 00000002 | Kill | 0        |
| <target3> | 00000003 | Kill | 0        |
| <target4> | 00000004 | Mine | 1        |
And I have entities
| !this           | ID       | Name                | IsLockedTarget |
| <targetEntity1> | 00000001 | Asteroid (Scordite) | false          |
| <targetEntity2> | 00000002 | Serpentis Spy       | false          |
| <targetEntity3> | 00000003 | Serpentis Watchman  | false          |
| <targetEntity4> | 00000004 | Asteroid (Veldspar) | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
And whether or not I am in a non-combat mode is 'false'
And my ship can lock '3' targets
# 3 pulses (2 targets + unlock buffer)
When I process queued targets '5' times
Then target '<targetEntity2>' should have been locked
And target '<targetEntity3>' should have been locked
And exactly '0' locked target(s) should be unlocked

Scenario: Change targets 1 - 5 entity, 5 queued, 2 type, max 4 - Should unlock an asteroid in favor of an npc
Given I have queued targets
| !this     | Id       | Type | Priority |
| <target1> | 00000001 | Mine | 1        |
| <target2> | 00000002 | Mine | 1        |
| <target3> | 00000003 | Mine | 1        |
| <target4> | 00000004 | Mine | 1        |
| <target5> | 00000005 | Kill | 0        |
| <target6> | 00000006 | Kill | 0        |
And I have entities
| !this           | ID       | Name                | IsLockedTarget |
| <targetEntity1> | 00000001 | Asteroid (Scordite) | true           |
| <targetEntity2> | 00000002 | Asteroid (Veldspar) | true           |
| <targetEntity3> | 00000003 | Asteroid (Veldspar) | true           |
| <targetEntity4> | 00000004 | Asteroid (Veldspar) | true           |
| <targetEntity5> | 00000005 | Serpentis Spy       | false          |
| <targetEntity6> | 00000006 | Serpentis Watchman  | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
And whether or not I am in a non-combat mode is 'true'
And my ship can lock '4' targets
# 3 pulses (2 targets + unlock buffer)
When I process queued targets '10' times
Then exactly '2' locked target(s) should be unlocked
And target '<targetEntity5>' should have been locked
And target '<targetEntity6>' should have been locked

Scenario: Change targets 2 - 5 entity, 5 queued, 2 type, max 4 - Should unlock a kill target in favor of an salvage target
Given I have queued targets
| !this     | Id       | Type        | Priority |
| <target1> | 00000001 | Kill        | 1        |
| <target2> | 00000002 | Kill        | 1        |
| <target3> | 00000003 | Kill        | 1        |
| <target4> | 00000004 | Kill        | 1        |
| <target5> | 00000005 | LootSalvage | 2        |
| <target6> | 00000006 | LootSalvage | 2        |
And I have entities
| !this           | ID       | Name                       | IsLockedTarget |
| <targetEntity1> | 00000001 | Serpentis Spy              | true           |
| <targetEntity2> | 00000002 | Serpentis Spy              | true           |
| <targetEntity3> | 00000003 | Serpentis Watchman         | true           |
| <targetEntity4> | 00000004 | Serpentis Watchman         | true           |
| <targetEntity5> | 00000005 | Wreck (Serpentis Watchman) | false          |
| <targetEntity6> | 00000006 | Cargo Container            | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
And whether or not I am in a non-combat mode is 'true'
And my ship can lock '4' targets
# 3 pulses (2 targets + unlock buffer)
When I process queued targets '10' times
Then exactly '2' locked target(s) should be unlocked
And target '<targetEntity5>' should have been locked
And target '<targetEntity6>' should have been locked

Scenario: Change targets 3 - 5 entity, 5 queued, 2 type, max 4 - Should 
Given I have queued targets
| !this     | Id       | Type | Priority |
| <target1> | 00000001 | Mine | 1        |
| <target2> | 00000002 | Mine | 1        |
| <target3> | 00000003 | Mine | 1        |
| <target4> | 00000004 | Mine | 1        |
| <target5> | 00000005 | Kill | 0        |
| <target6> | 00000006 | Kill | 0        |
And I have entities
| !this           | ID       | Name                | IsLockedTarget |
| <targetEntity1> | 00000001 | Asteroid (Scordite) | true           |
| <targetEntity2> | 00000002 | Asteroid (Veldspar) | true           |
| <targetEntity3> | 00000003 | Asteroid (Veldspar) | true           |
| <targetEntity4> | 00000004 | Asteroid (Veldspar) | true           |
| <targetEntity5> | 00000005 | Serpentis Spy       | false          |
| <targetEntity6> | 00000006 | Serpentis Watchman  | false          |
And I have modules
| !this     | ID       | IsActive | TargetID |
| <module1> | 00000001 | true     | 00000001 |
| <module2> | 00000002 | true     | 00000002 |
And whether or not I am in a non-combat mode is 'true'
And my ship can lock '4' targets
# 3 pulses (2 targets + unlock buffer)
When I process queued targets '10' times
Then exactly '2' locked target(s) should be unlocked
And target '<targetEntity5>' should have been locked
And target '<targetEntity6>' should have been locked
And target '<targetEntity1>' should not have been unlocked
And target '<targetEntity2>' should not have been unlocked