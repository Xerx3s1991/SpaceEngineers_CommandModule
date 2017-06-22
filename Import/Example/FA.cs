// -------------------------------------------------- 
// ---------- Flight Assist ------------------------- 
// -------------------------------------------------- 
// Version 3.2 
// 
// Computer assisted flight 
// Control your ship without thrusters in every direction 
//  
// Author: Naosyth  
// naosyth@gmail.com  
 
// Todo: 
// Implement Lostkangaroo's other suggestions (Already added debug class) 
// Make orientation code better 
// Optimize using new PB additions 
 
// -------------------------------------------------- 
// ---------- Configuration ------------------------- 
// -------------------------------------------------- 
 
// ----- Block Names ----- 
//   Name of remote control block. Names are CaSe SeNsItIvE! 
const string RemoteControlBlockName = "FA Remote"; 
 
// ----- Main Thrust Orientation ----- 
// Configure which side of the ship has the main thrusters for flight in space. 
// Accepted values are 'forward', 'backward', 'right', 'left', 'up', and 'down' 
const string SpaceMainThrust = "backward"; 
 
// ----- Gyro Configuration ----- 
// GyroCount - Number of gyros to use for orienting the ship 
const int GyroCount = 4; 
 
// ----- Hover Assist Configuration ----- 
// If set to true, the hover assist module will always be enabled while in gravity 
const bool AlwaysEnabledInGravity = false; 
 
// GyroResponsiveness - Adjust gyro responce curve. High = smooth but slower response. 
const int GyroResponsiveness = 8; 
 
// GyroVelocityScale - Scales gyro RPM. If you are overcorrecting, try lowering this. 
const double GyroVelocityScale = 0.2; 
 
// MaxPitch / MaxRoll - Maximum angle reached by the HoverAssist module when stopping the vehicle 
const double MaxPitch = 45; 
const double MaxRoll = 45; 
 
// ----- Printer Module Configuration ----- 
// The number of ticks that must pass inbetween text panel screen re-draws 
const int ScreenDrawInterval = 5; 
 
// ----- Horizon Display Configuration ----- 
// Height and width of the horizon display 
const int HorizonHeight = 13; 
const int HorizonWidth = 27; 
 
// -------------------------------------------------- 
// ---------- Program ------------------------------- 
// -------------------------------------------------- 
const bool DebugMode = false; 
const string DebugScreen = "Debug Screen"; 
 
const double HalfPi = Math.PI / 2; 
const double RadToDeg = 180 / Math.PI; 
const double DegToRad = Math.PI / 180; 
 
bool initialized = false; 
FlightAssist fa; 
Debug debug; 
 
void Main(string arguments) { 
  if (!initialized) { 
    fa = new FlightAssist(GridTerminalSystem, Me); 
    debug = new Debug(GridTerminalSystem); 
    initialized = true; 
  } 
 
  fa.Run(arguments); 
} 
 
public class Debug { 
  private static IMyTextPanel debugScreen; 
 
  public Debug(IMyGridTerminalSystem grid) { 
    if (DebugMode) { 
      debugScreen = (IMyTextPanel)grid.GetBlockWithName(DebugScreen); 
      debugScreen.WritePublicText("Flight Assist Debug"); 
    } 
  } 
 
  public static void Append(string msg) { 
    if (DebugMode) { 
      string output = msg; 
      output += "\n" + debugScreen.GetPublicText(); 
      output = output.Substring(0, Math.Min(output.Length, 500)); 
      debugScreen.ShowTextureOnScreen(); 
      debugScreen.WritePublicText(output); 
      debugScreen.ShowPublicTextOnScreen(); 
    } 
  } 
} 
 
class FlightAssist { 
  private List<Module> modules; 
 
  public static IMyProgrammableBlock Me; 
  public static IMyGridTerminalSystem GridTerminalSystem; 
 
  public static Delta delta; 
  public static Printer printer; 
  public static HoverAssist hoverAssist; 
  public static VectorAssist vectorAssist; 
 
  public static Helpers helpers; 
 
  public FlightAssist(IMyGridTerminalSystem gts, IMyProgrammableBlock pb) { 
    GridTerminalSystem = gts; 
    Me = pb; 
 
    helpers = new Helpers(); 
 
    modules = new List<Module>(); 
    modules.Add(delta = new Delta("Delta")); 
    modules.Add(hoverAssist = new HoverAssist("HoverAssist")); 
    modules.Add(vectorAssist = new VectorAssist("VectorAssist")); 
    modules.Add(printer = new Printer("Printer")); 
  } 
 
  public void Run(string arguments) { 
    string[] args = arguments.Split(' '); 
    for (var i = 0; i < modules.Count; i++) 
      modules[i].Run(args); 
  } 
} 
 
abstract class Module { 
  public string name; 
  private string printBuffer; 
 
  public Module(string n) { 
    name = n; 
  }  
 
  virtual public void Run(string[] args) { 
    printBuffer = ""; 
    if (args[0] == name) 
      ProcessCommand(args); 
    else if (args[0] == "") 
      Tick(); 
  } 
 
  abstract public void Tick(); 
  virtual public void ProcessCommand(string[] args) {} 
  public string GetPrintString() { return printBuffer; } 
  protected void PrintLine(string line) { printBuffer += line + "\n"; } 
} 
 
class Printer : Module { 
  private List<ScreenState> screens; 
 
  private List<Module> printableModules; 
  private int numModules; 
 
  private class ScreenState { 
    public IMyTextPanel screen; 
    public int module; 
  } 
 
  // Text panel configuration. First argument of ScreenConfig is the block name 
  // Second argument is the number of the module to display by default. 
  // Module numbers start at 0. By default, 0 is HoverAssist and 1 is VectorAssist 
  // e.g. 
  // public readonly List<ScreenConfig> TextPanelInfo = new List<ScreenConfig> { 
  //   new ScreenConfig("FA Screen 1", 1), 
  //   new ScreenConfig("FA Screen 2", 0) 
  // }; 
  // Note that the last element does not have a comma at the end. 
  // Screens are totally optional, and if the screen is not found, the program 
  // will not crash, it just won't print anything. 
  private List<ScreenConfig> TextPanelInfo = new List<ScreenConfig> { 
    new ScreenConfig("FA Screen", 2) 
  }; 
 
  public Printer(string name) : base(name) { 
    screens = new List<ScreenState>(); 
 
    for (var i = 0; i < TextPanelInfo.Count; i++) { 
      IMyTextPanel s = FlightAssist.GridTerminalSystem.GetBlockWithName(TextPanelInfo[i].Name) as IMyTextPanel; 
      if (s != null) 
        screens.Add(new ScreenState { screen = s, module = TextPanelInfo[i].Module }); 
    } 
 
    printableModules = new List<Module>(); 
    printableModules.Add(FlightAssist.hoverAssist); 
    printableModules.Add(FlightAssist.vectorAssist); 
    numModules = printableModules.Count; 
  } 
 
  override public void ProcessCommand(string[] args) { 
    if (args == null) 
      return; 
 
    Debug.Append("Command: " + args[1]); 
 
    var command = args[1].ToLower(); 
    var index = (args.Length >= 3) ? Int32.Parse(args[2]) : 0; 
    if (index >= printableModules.Count) return; 
    if (index < 0 || index >= screens.Count) return; 
 
    switch (command) { 
      case "next": 
        screens[index].module += 1; 
        break; 
      case "previous": 
        screens[index].module -= 1; 
        break; 
    } 
  } 
 
  override public void Tick() { 
    for (var i = 0; i < screens.Count; i++) { 
      var screen = screens[i].screen; 
      var module = printableModules[Math.Abs(screens[i].module) % numModules]; 
      screen.ShowTextureOnScreen(); 
      screen.WritePublicText("Flight Assist - Module [" + module.name + "]"); 
      screen.WritePublicText("\n" + module.GetPrintString(), true); 
      screen.ShowPublicTextOnScreen(); 
    } 
  } 
} 
 
class Delta : Module { 
  private string remoteControlName = RemoteControlBlockName; 
  private int gyroCount = GyroCount; 
  private double gyroVelocityScale = GyroVelocityScale; 
  private double minGyroRpmScale = 0.001; 
 
  private bool gyrosEnabled; 
  public IMyRemoteControl remote; 
  public List<IMyGyro> gyros; 
 
  private Vector3D oldPosition; 
  public Vector3D position; 
  public Vector3D deltaPosition; 
  public double speed; 
  public double localSpeedForward, localSpeedRight, localSpeedUp; 
 
  public Matrix shipOrientation; 
  public Matrix worldOrientation; 
 
  public Vector3D gravity; 
  public bool inGravity; 
  public bool switchingGravity; 
 
  private Vector3D reference, target; 
  public double angle; 
  private double dt = 1000/60; 
 
  public Delta(string name) : base(name) { 
    var list = new List<IMyTerminalBlock>(); 
    FlightAssist.GridTerminalSystem.GetBlocksOfType<IMyGyro>(list, x => x.CubeGrid == FlightAssist.Me.CubeGrid); 
    gyros = list.ConvertAll(x => (IMyGyro)x); 
 
    if (gyros.Count < gyroCount)  
      throw new Exception("\nScript is configured to use " + gyroCount + " gyros, but only detected " + gyros.Count + " on the vehicle."); 
 
    gyros = gyros.GetRange(0, gyroCount); 
 
    remote = FlightAssist.GridTerminalSystem.GetBlockWithName(remoteControlName) as IMyRemoteControl; 
    if (remote == null) FlightAssist.helpers.MissingBlockException("Remote Control", remoteControlName); 
 
    remote.Orientation.GetMatrix(out shipOrientation); 
  } 
 
  override public void Tick() { 
    CalcVelocity(); 
    CalcGravity(); 
    worldOrientation = remote.WorldMatrix; 
 
    UpdateGyroRpm(); 
  } 
 
  private void CalcVelocity() { 
    position = remote.GetPosition(); 
    deltaPosition = position - oldPosition; 
    oldPosition = position; 
    speed = deltaPosition.Length() / dt * 1000; 
    deltaPosition.Normalize(); 
 
    localSpeedUp = FlightAssist.helpers.NotNan(Vector3D.Dot(deltaPosition, worldOrientation.Up) * speed); 
    localSpeedRight = FlightAssist.helpers.NotNan(Vector3D.Dot(deltaPosition, worldOrientation.Right) * speed); 
    localSpeedForward = FlightAssist.helpers.NotNan(Vector3D.Dot(deltaPosition, worldOrientation.Forward) * speed); 
  } 
 
  private void CalcGravity() { 
    gravity = -Vector3D.Normalize(remote.GetNaturalGravity()); 
    switchingGravity = inGravity; 
    inGravity = !double.IsNaN(gravity.X); 
    switchingGravity = (inGravity != switchingGravity); 
  } 
 
  public void ToggleGyros(bool state) { 
    gyrosEnabled = state; 
    for (int i = 0; i < gyros.Count; i++) 
      gyros[i].SetValueBool("Override", gyrosEnabled); 
  } 
 
  public void SetTargetOrientation(Vector3D setReference, Vector3D setTarget) { 
    reference = setReference; 
    target = setTarget; 
    UpdateGyroRpm(); 
  } 
 
  private void UpdateGyroRpm() { 
    if (!gyrosEnabled) return; 
 
    for (int i = 0; i < gyros.Count; i++) { 
      var g = gyros[i]; 
 
      Matrix localOrientation; 
      g.Orientation.GetMatrix(out localOrientation); 
      var localReference = Vector3D.Transform(reference, MatrixD.Transpose(localOrientation)); 
      var localTarget = Vector3D.Transform(target, MatrixD.Transpose(g.WorldMatrix.GetOrientation())); 
 
      var axis = Vector3D.Cross(localReference, localTarget); 
      angle = axis.Length(); 
      angle = Math.Atan2(angle, Math.Sqrt(Math.Max(0.0, 1.0 - angle * angle))); 
      if (Vector3D.Dot(localReference, localTarget) < 0) 
        angle = Math.PI; 
      axis.Normalize(); 
      axis *= Math.Max(minGyroRpmScale, g.GetMaximum<float>("Roll") * (angle / Math.PI) * gyroVelocityScale); 
 
      g.SetValueFloat("Pitch", (float)axis.X); 
      g.SetValueFloat("Yaw", (float)-axis.Y); 
      g.SetValueFloat("Roll", (float)-axis.Z); 
    } 
  } 
} 
 
class VectorAssist : Module { 
  private double angleThreshold = 0.01; 
  private double speedThreshold = 0.3; 
 
  private Vector3D thrustVector; 
 
  private string action; 
  private double startSpeed; 
  private int brakeHoldTimer; 
 
  public VectorAssist(string name) : base(name) { 
    thrustVector = FlightAssist.helpers.GetThrustVector(SpaceMainThrust); 
    action = ""; 
  } 
 
  override public void Tick() { 
    PerformActions(); 
    BuildVisual(); 
  } 
 
  override public void ProcessCommand(string[] args) { 
    var command = args[1].ToLower(); 
 
    if (action == command) { 
      FlightAssist.delta.ToggleGyros(false); 
      action = ""; 
      return; 
    } 
 
    action = command; 
    switch (action) { 
      case "brake": 
        SpaceBrake(true); 
        break; 
    } 
  } 
 
  private void PerformActions() { 
    if (action == "") 
      return; 
 
    FlightAssist.delta.ToggleGyros(true); 
 
    switch (action) { 
      case "brake": 
        SpaceBrake(); 
        break; 
 
      case "prograde": 
        TargetOrientation(-FlightAssist.delta.deltaPosition); 
        break; 
 
      case "retrograde": 
        TargetOrientation(FlightAssist.delta.deltaPosition); 
        break; 
    } 
  } 
 
  private void BuildVisual() { 
    int height = HorizonHeight; 
    int width = HorizonWidth; 
 
    int yCenter = height/2; 
    int xCenter = width/2; 
 
    double pitch, tilt, roll = 0; 
    if (FlightAssist.delta.inGravity) { 
      pitch = FlightAssist.helpers.NotNan(Math.Acos(Vector3D.Dot(FlightAssist.delta.worldOrientation.Forward, FlightAssist.delta.gravity)) * RadToDeg); 
      pitch = height - Math.Floor((pitch / 180) * height) - 1; 
 
      tilt = Math.Floor(Vector3D.Dot(FlightAssist.delta.worldOrientation.Right, FlightAssist.delta.deltaPosition) * xCenter) + xCenter; 
      if (FlightAssist.delta.localSpeedForward < 0) tilt = width - tilt; 
 
      roll = Math.Asin(Vector3.Dot(FlightAssist.delta.gravity, FlightAssist.delta.worldOrientation.Right)) * RadToDeg; 
    } else { 
      pitch = -Math.Floor((FlightAssist.delta.localSpeedUp / FlightAssist.delta.speed) * yCenter) + yCenter; 
      tilt = Math.Floor((FlightAssist.delta.localSpeedRight / FlightAssist.delta.speed) * xCenter) + xCenter; 
    } 
 
    var upsideDown = Vector3D.Dot(FlightAssist.delta.worldOrientation.Up, FlightAssist.delta.gravity) < 0; 
 
    double horizon, horizonPoint; 
    horizon = horizonPoint = pitch; 
 
    string output = ""; 
    for (var y = 0; y < height; y++) { 
      output += "       |"; 
      for (var x = 0; x < width; x++) { 
        if (FlightAssist.delta.inGravity) { 
          if (FlightAssist.helpers.EqualWithMargin(roll, 0, 0.03)) { 
            horizonPoint = horizon; 
          } else { 
            horizonPoint = Math.Floor((height * 2 * roll / 90)) * (x - xCenter) / (width/2) + horizon; 
            if (upsideDown) horizonPoint = height - horizonPoint; 
          } 
        } 
 
        if (FlightAssist.delta.inGravity && (x == tilt && y == 0)) 
          output += ".!"; 
        else if (!FlightAssist.delta.inGravity && x == tilt && y == pitch) 
          output += FlightAssist.delta.localSpeedForward < 0 ? "~" : "+"; 
        else if (x == xCenter && y == yCenter) 
          output += "  "; 
        else if (x == xCenter-1 && y == yCenter) 
          output += "<"; 
        else if (x == xCenter+1 && y == yCenter) 
          output += ">"; 
        else if (FlightAssist.delta.inGravity && !upsideDown && y > horizonPoint) 
          output += "="; 
        else if (FlightAssist.delta.inGravity && upsideDown && y < horizonPoint) 
          output += "="; 
        else 
          output += ". "; 
      } 
      output += "|\n"; 
    } 
 
    PrintLine(output); 
 
    if (action == "brake") { 
      var percent = Math.Abs(FlightAssist.delta.speed / startSpeed); 
      output = "       |"; 
      for (var i = 0; i < width; i++) 
        output += (i < width * percent) ? "=" : "~"; 
      output += "|\n       |  Braking In Progress                       |"; 
    } else { 
      output = "       Speed: " + Math.Abs(FlightAssist.delta.speed).ToString("000") + " m/s"; 
    } 
 
    PrintLine(output); 
  } 
 
  private void TargetOrientation(Vector3D target) { 
    FlightAssist.delta.SetTargetOrientation(thrustVector, target); 
 
    if (Vector3D.Dot(thrustVector, target) > 0 && FlightAssist.helpers.EqualWithMargin(FlightAssist.delta.angle, 0, angleThreshold)) { 
      FlightAssist.delta.ToggleGyros(false); 
      action = ""; 
    } 
  } 
 
  private void SpaceBrake(bool setup = false) { 
    if (setup) { 
      brakeHoldTimer = 0; 
      startSpeed = FlightAssist.delta.speed; 
      if (FlightAssist.delta.remote.DampenersOverride) 
        FlightAssist.delta.remote.GetActionWithName("DampenersOverride").Apply(FlightAssist.delta.remote); 
    } 
 
    if (FlightAssist.delta.inGravity) { 
      action = ""; 
      return; 
    } 
 
    FlightAssist.delta.SetTargetOrientation(thrustVector, FlightAssist.delta.deltaPosition); 
 
    // Activate dampeners when on target, if they aren't already on. 
    if (!FlightAssist.delta.remote.DampenersOverride && FlightAssist.helpers.EqualWithMargin(FlightAssist.delta.angle, 0, angleThreshold)) { 
      brakeHoldTimer += 1; 
    } 
 
    if (brakeHoldTimer >= 60) { 
      FlightAssist.delta.remote.GetActionWithName("DampenersOverride").Apply(FlightAssist.delta.remote); 
      brakeHoldTimer = 0; 
    } 
 
    // Stop when velocity is nearly 0 
    if (FlightAssist.delta.speed < speedThreshold) { 
      FlightAssist.delta.ToggleGyros(false); 
      action = ""; 
      return; 
    } 
  } 
} 
 
class HoverAssist : Module { 
  private bool alwaysEnabledInGravity = AlwaysEnabledInGravity; 
  private int gyroResponsiveness = GyroResponsiveness; 
  private double maxPitch = MaxPitch; 
  private double maxRoll = MaxRoll; 
 
  private bool hoverEnabled; 
  private string mode; 
  private float setSpeed; 
 
  private double pitch, roll; 
  private double desiredPitch, desiredRoll; 
  private double worldSpeedForward, worldSpeedRight, worldSpeedUp; 
 
  public HoverAssist(string name) : base(name) { 
    alwaysEnabledInGravity = AlwaysEnabledInGravity; 
    gyroResponsiveness = GyroResponsiveness; 
 
    mode = "Hover"; 
    hoverEnabled = FlightAssist.delta.gyros[0].GyroOverride; 
    FlightAssist.delta.ToggleGyros(hoverEnabled); 
  } 
 
  override public void Tick() { 
    if (!FlightAssist.delta.inGravity) 
      hoverEnabled = false; 
    else if (alwaysEnabledInGravity && hoverEnabled == false) 
      hoverEnabled = true; 
 
    CalcWorldSpeed(); 
    CalcPitchAndRoll(); 
 
    ExecuteManeuver(); 
    PrintStatus(); 
    PrintVelocity(); 
    PrintOrientation(); 
  } 
 
  override public void ProcessCommand(string[] args) { 
    var command = args[1]; 
    if (FlightAssist.delta.inGravity) { 
      switch (command.ToLower()) { 
        case "toggle": 
          hoverEnabled = !hoverEnabled; 
          FlightAssist.delta.ToggleGyros(false); 
          break; 
 
        case "cruise": 
          setSpeed = args[2] != null ? Int32.Parse(args[2]) : 0; 
          mode = "Cruise"; 
          break; 
 
        default: 
          mode = command; 
          break; 
      } 
    } 
  } 
 
  private void PrintStatus() { 
    PrintLine("----- Status -------------------------------------------"); 
    PrintLine("Hover State: " + (hoverEnabled ? "ENABLED" : "DISABLED")); 
    PrintLine("Hover Mode: " + mode.ToUpper()); 
  } 
 
  private void PrintVelocity() { 
    PrintLine("\n----- Velocity ----------------------------------------"); 
    PrintLine("  F/B: " + worldSpeedForward.ToString("+000;\u2013000")); 
    PrintLine("  R/L: " + worldSpeedRight.ToString("+000;\u2013000")); 
    PrintLine("  U/D: " + worldSpeedUp.ToString("+000;\u2013000")); 
  } 
 
  private void PrintOrientation() { 
    PrintLine("\n----- Orientation ----------------------------------------"); 
    PrintLine("Pitch: " + pitch.ToString("+00;\u201300") + "° | " + roll.ToString("+00;\u201300") + "°"); 
    PrintLine("Pitch: " + desiredPitch.ToString("+00;\u201300") + "° | " + desiredRoll.ToString("+00;\u201300") + "°"); 
  } 
 
  private void CalcWorldSpeed() { 
    worldSpeedForward = FlightAssist.helpers.NotNan(Vector3D.Dot(FlightAssist.delta.deltaPosition, Vector3D.Cross(FlightAssist.delta.gravity, FlightAssist.delta.worldOrientation.Right)) * FlightAssist.delta.speed); 
    worldSpeedRight = FlightAssist.helpers.NotNan(Vector3D.Dot(FlightAssist.delta.deltaPosition, Vector3D.Cross(FlightAssist.delta.gravity, FlightAssist.delta.worldOrientation.Forward)) * FlightAssist.delta.speed); 
    worldSpeedUp = FlightAssist.helpers.NotNan(Vector3D.Dot(FlightAssist.delta.deltaPosition, FlightAssist.delta.gravity)); 
  } 
 
  private void CalcPitchAndRoll() { 
    pitch = FlightAssist.helpers.NotNan(Math.Acos(Vector3D.Dot(FlightAssist.delta.worldOrientation.Forward, FlightAssist.delta.gravity)) * RadToDeg); 
    roll = FlightAssist.helpers.NotNan(Math.Acos(Vector3D.Dot(FlightAssist.delta.worldOrientation.Right, FlightAssist.delta.gravity)) * RadToDeg); 
  } 
 
  private void ExecuteManeuver() { 
    if (!hoverEnabled) 
      return; 
 
    FlightAssist.delta.ToggleGyros(true); 
 
    switch (mode.ToLower()) { 
      case "glide": 
        desiredPitch = 0; 
        desiredRoll = Math.Atan(worldSpeedRight / gyroResponsiveness) / HalfPi * maxRoll; 
        break; 
 
      case "freeglide": 
        desiredPitch = 0; 
        desiredRoll = 0; 
        break; 
 
      case "pitch": 
        desiredPitch = Math.Atan(worldSpeedForward / gyroResponsiveness) / HalfPi * maxPitch; 
        desiredRoll = (roll - 90); 
        break; 
 
      case "roll": 
        desiredPitch = -(pitch - 90); 
        desiredRoll = Math.Atan(worldSpeedRight / gyroResponsiveness) / HalfPi * maxRoll; 
        break; 
 
      case "cruise": 
        desiredPitch = Math.Atan((worldSpeedForward - setSpeed) / gyroResponsiveness) / HalfPi * maxPitch; 
        desiredRoll = Math.Atan(worldSpeedRight / gyroResponsiveness) / HalfPi * maxRoll; 
        break; 
 
      default: // Stationary Hover 
        desiredPitch = Math.Atan(worldSpeedForward / gyroResponsiveness) / HalfPi * maxPitch; 
        desiredRoll = Math.Atan(worldSpeedRight / gyroResponsiveness) / HalfPi * maxRoll; 
        break; 
    } 
 
    var quatPitch = Quaternion.CreateFromAxisAngle(FlightAssist.delta.shipOrientation.Left, (float)(desiredPitch * DegToRad)); 
    var quatRoll = Quaternion.CreateFromAxisAngle(FlightAssist.delta.shipOrientation.Backward, (float)(desiredRoll * DegToRad)); 
    var reference = Vector3D.Transform(FlightAssist.delta.shipOrientation.Down, quatPitch * quatRoll); 
    FlightAssist.delta.SetTargetOrientation(reference, FlightAssist.delta.remote.GetNaturalGravity()); 
  } 
} 
 
// -------------------------------------------------- 
// ---------- Helper Functions ---------------------- 
// -------------------------------------------------- 
public class Helpers { 
  public bool EqualWithMargin(double value, double target, double margin) { 
    return value > target - margin && value < target + margin; 
  } 
 
  public Vector3D GetThrustVector(string direction) { 
    switch (direction.ToLower()) { 
      case "down": return FlightAssist.delta.shipOrientation.Down; 
      case "up": return FlightAssist.delta.shipOrientation.Up; 
      case "forward": return FlightAssist.delta.shipOrientation.Forward; 
      case "backward": return FlightAssist.delta.shipOrientation.Backward; 
      case "right": return FlightAssist.delta.shipOrientation.Right; 
      case "left": return FlightAssist.delta.shipOrientation.Left; 
      default: throw new Exception("Unidentified thrust direction '" + direction.ToLower() + "'"); 
    } 
  } 
 
  public double NotNan(double val) { 
    if (double.IsNaN(val)) return 0; 
    return val; 
  } 
 
  public void MissingBlockException(string type, string name) { 
    throw new Exception(type + " named '" + name + "' not found."); 
  } 
} 
 
public class ScreenConfig { 
  public ScreenConfig(string name, int module) { 
    this.Name = name; 
    this.Module = module; 
  } 
 
  public string Name { get; set; } 
  public int Module { get; set; } 
} 
