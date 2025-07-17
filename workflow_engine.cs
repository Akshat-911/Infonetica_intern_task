// class for a single state in a workflow
public class State
{
    public string Id { get; set; }             // Unique id for the state
    public bool IsInitial { get; set; }        // check if this is the starting state, true if it is and false otherwise
    public bool IsFinal { get; set; }          // check if  this is the ending state
    public bool Enabled { get; set; }          // check whether the state is currently enabled
    public string? Description { get; set; }   // for future extensibility: Providing optional details for states or workflows, useful for documentation or UI display
}

// class for a transition/Action from one state to another
public class WorkflowAction
{
    public string Id { get; set; }                     // Unique id for the action
    public bool Enabled { get; set; }                  // check if the action is allowed
    public List<string> FromStates { get; set; } = new();  // States which are allowed to do this action
    public string ToState { get; set; }                // Target state of the transition
    public string? Label { get; set; }                 // for future extensibility: name/text(can be null/empty) shown to users on buttons/dropdowns etc on the UI , helps in giving a user friendly experience
}

// class for the overall structure/blueprint of a workflow
public class WorkflowDefinition
{
    public string Id { get; set; }                      // Unique id for the workflow definition
    public List<State> States { get; set; } = new();    // List of states in the workflow
    public List<WorkflowAction> Actions { get; set; } = new();  // List of allowed transitions/actions
    public string? Description { get; set; }            // for future extensibility: can be useful for documentation, UI display
}

// class for a running instance of a workflow
public class WorkflowInstance
{
    public string Id { get; set; }                                        // Unique id for this instance
    public string WorkflowDefinitionId { get; set; }                      // Reference to its workflow definition
    public string CurrentState { get; set; }                              // Current state of the instance
    public List<(string ActionId, DateTime Timestamp)> History { get; set; } = new();  // History of transitions
}

// For this assignment I havent used any database instead used in-memory storage with help of dictionaries for simplicity and maintainability.
var workflowDefinitions = new Dictionary<string, WorkflowDefinition>(); // stores workflow definitions by ID
var workflowInstances = new Dictionary<string, WorkflowInstance>();     // stores workflow instances by ID

var builder = WebApplication.CreateBuilder(args); // Sets up and builds the web application using default configurations.
var app = builder.Build();

// API to create a new workflow definition
app.MapPost("/workflow-definitions", (WorkflowDefinition definition) =>
{
    if (workflowDefinitions.ContainsKey(definition.Id)) //checking if the workflow definition already exist in the dictionary
        return Results.BadRequest("Workflow definition ID already exists.");

    if (definition.States.Count(s => s.IsInitial) != 1) //making sure the workflow has only 1 initial state
        return Results.BadRequest("A workflow must have exactly one initial state.");

    var duplicateStates = definition.States.GroupBy(s => s.Id).Where(g => g.Count() > 1).Select(g => g.Key);
    if (duplicateStates.Any()) //checking for duplicate states
        return Results.BadRequest($"Duplicate state IDs found: {string.Join(", ", duplicateStates)}");

    var duplicateActions = definition.Actions.GroupBy(a => a.Id).Where(g => g.Count() > 1).Select(g => g.Key);
    if (duplicateActions.Any()) //checking for duplicate actions
        return Results.BadRequest($"Duplicate action IDs found: {string.Join(", ", duplicateActions)}");

    workflowDefinitions[definition.Id] = definition; //adding to in-memory dictionary
    return Results.Ok("Workflow definition created successfully.");
});

// API to retrieve a workflow definition by ID
app.MapGet("/workflow-definitions/{id}", (string id) =>
{
    if (!workflowDefinitions.TryGetValue(id, out var definition)) //checking if the id of the definition exists in the dictionary
        return Results.NotFound("Workflow definition not found.");

    return Results.Ok(definition); //returning the workflow definition
});

// API to start a new instance of a workflow
app.MapPost("/workflow-definitions/{workflowId}/instances", (string workflowId) =>
{
    if (!workflowDefinitions.TryGetValue(workflowId, out var definition)) //checking if the workflow definition exists
        return Results.NotFound("Workflow definition not found.");

    var initialState = definition.States.FirstOrDefault(s => s.IsInitial && s.Enabled); //getting the initial enabled state
    if (initialState == null)
        return Results.BadRequest("No enabled initial state found."); //cannot create instance if no valid start state

    var instance = new WorkflowInstance
    {
        Id = Guid.NewGuid().ToString(),                   //generating unique ID for the instance
        WorkflowDefinitionId = workflowId,                //assigning definition ID
        CurrentState = initialState.Id                    //setting initial state
    };

    workflowInstances[instance.Id] = instance;            //adding to in-memory dictionary
    return Results.Ok(instance);                          //returning the new instance
});

// API to execute an action on a given workflow instance
app.MapPost("/instances/{instanceId}/actions/{actionId}", (string instanceId, string actionId) =>
{
    if (!workflowInstances.TryGetValue(instanceId, out var instance))
        return Results.NotFound("Workflow instance not found.");  //checking if the instance exists

    if (!workflowDefinitions.TryGetValue(instance.WorkflowDefinitionId, out var definition))
        return Results.NotFound("Workflow definition not found."); //checking if the definition exists

    var currentState = instance.CurrentState; //get current state of the instance
    var action = definition.Actions.FirstOrDefault(a => a.Id == actionId); //lookup action by ID

    if (action == null)
        return Results.BadRequest("Action does not exist in this workflow."); //action ID invalid

    if (!action.Enabled) //checking if the action is allowed
        return Results.BadRequest("Action is disabled.");

    if (!action.FromStates.Contains(currentState)) //checking whether the current state is one of the belonged states
        return Results.BadRequest("Action not valid from current state.");

    var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState); //get the destination state
    if (targetState == null || !targetState.Enabled)
        return Results.BadRequest("Target state is invalid or disabled."); //cannot move to invalid/disabled state

    var stateMeta = definition.States.FirstOrDefault(s => s.Id == currentState);
    if (stateMeta != null && stateMeta.IsFinal)
        return Results.BadRequest("Cannot transition from a final state."); //final states cannot be exited

    instance.CurrentState = action.ToState; //update current state
    instance.History.Add((actionId, DateTime.UtcNow)); //add to history log

    return Results.Ok(instance); //return updated instance
});

// API to get workflow instance state and history
app.MapGet("/instances/{id}", (string id) =>
{
    if (!workflowInstances.TryGetValue(id, out var instance))
        return Results.NotFound("Workflow instance not found."); //checking if instance exists

    return Results.Ok(instance); //returning full instance data including history
});

app.Run(); //starts the application and begins listening for HTTP requests
