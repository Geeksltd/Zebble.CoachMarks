[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.CoachMarks/master/Zebble.CoachMarks/Shared/Icon.png "Zebble.CoachMarks"


## Zebble.CoachMarks

![logo]

CoachMarks is a plugin for Zebble apps to show a brief help to the users.


[![NuGet](https://img.shields.io/nuget/v/Zebble.CoachMarks.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.CoachMarks/)

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.CoachMarks/](https://www.nuget.org/packages/Zebble.CoachMarks/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
* After installing the Nuget copy this [SCSS](https://raw.githubusercontent.com/Geeksltd/Zebble.CoachMarks/master/Shared/CoachMarks.scss/) file to your project.
* Import the SCSS file to `common.scss` file.
```scss
@import "CoachMarks.scss";
```
* Call its mixin in the `common.scss` just like the other mixins.
```scss
@include coach-marks($navbar-height);
```
* As it depends on [PopOver](https://github.com/Geeksltd/Zebble.PopOver), you should follow its setup steps as well.

<br>


### Api Usage


```csharp
var settings = new CoachMarks.Settings
{
  DisableRealEvents = true,
  TopButtons = Buttons.Skip,
  BottomButtons = Buttons.Next | Buttons.Back
};

var coach = new CoachMarks(settings);

coach.CreateStep("Tap this button to skip this part.", SkipButton.Id);
coach.CreateStep("When you are not 100% sure tap this button.", NotSureButton.Id);
coach.CreateStep("You could find more feature here.", "MenuButton");

await coach.Coach();
```
As the coaching would take time, it would be a good idea to call the Coach method without using await keyword when something is going out.
```csharp
var coach = new CoachMarks();

coach.Settings.MoveOnByTime = true;
coach.CreateStep("Tap this button to skip this part.", SkipButton.Id);

coach.Coach();
```

### Setting

| Property          | Type              | decvription |
| :-----------      | :-----------      | :------ |
| MoveOnByTime      | bool              | It is used for moving on the steps when the specific time passes. |
| Delay             | TimeSpan          | The specific time for the MoveOnByTime.          |
| ElementPadding    | int               | The element you are pointed at for any steps would be held in a container and its the padding of the holder.|
| DisableRealEvents | bool              | Disable the events to suppress any interaction in middle of the coach time.          |
| TopButtons        | CoachMarks.Buttons| There are two default sections to add buttons (Skip, Next and Back).          |
| BottomButtons     | CoachMarks.Buttons| There are two default sections to add buttons (Skip, Next and Back).          |

<br>


### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| IsCoaching   | bool         | x       | x   | x       |
| Settings   | CoachMarks.Settings         | x       | x   | x       |



<br>


### Exception
| Exception            | Reason                                          |
| :-----------      | :-----------                                  |
| InvalidOperationException | It happens when you call the Coach method while the coach is processing.    |


<br>


### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| Show        | Task         | CancellationToken => cancellationToken | x       | x   | x       |
| Hide        | void         |		| x       | x   | x       |
| CreateStep   | void         | string => text, string => elementId | x       | x   | x       |