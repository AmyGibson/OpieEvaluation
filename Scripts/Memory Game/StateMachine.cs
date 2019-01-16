/* A simple state machine class */


using System;
using System.Collections.Generic;

public class State
{
	public delegate void EventCallback(State state, Event ev);
	public delegate void UpdateCallback(State state);

	public EventCallback onEnter;
	public EventCallback onExit;
	public UpdateCallback onUpdate;

	string _name;
	public string name { get { return _name; } private set { _name = value; } }

	public State(string name)
	{
		this.onEnter = (State state, Event ev) => {};
		this.onExit = (State state, Event ev) => {};
		this.onUpdate = (State State) => {};
		this.name = name;
	}
}

public class Event
{
	string _name;
	public string name { get { return _name; } private set { _name = value; } }

	public Event(string name)
	{
		this.name = name;
	}

}


public struct Transition
{
	public State from;
	public Event ev;
	public State to;
}

public class TransitionTable
{
	public Dictionary<State, Dictionary<Event, Transition> > transitions;

	public TransitionTable(Transition [] transitions)
	{
		this.transitions = new Dictionary<State, Dictionary<Event, Transition> > ();
		foreach (Transition transition in transitions) {


			if (!this.transitions.ContainsKey(transition.from))
			{
				this.transitions.Add (transition.from, new Dictionary<Event, Transition>());
			}

			this.transitions[transition.from].Add (transition.ev, transition);
		}
	}
}

public class StateMachine
{
	TransitionTable transition_table;

	State current_state;

	public State state {get { return current_state; } }

	public delegate void TransitionCallback(Transition transition);
	public delegate void UpdateCallback(State state);

	public TransitionCallback onTransition = (Transition Transition) => {};

	public UpdateCallback onUpdate = (State State) => {};

	uint update_immediate_nest_level = 0;

	bool inited = false;

	Queue<Event> delayed_events = new Queue<Event>();

	public StateMachine (Transition [] transitions)
	{
		transition_table = new TransitionTable (transitions);
	}

	public void init(State initial)
	{
		Event initial_event = new Event ("init");
		current_state = initial;
		current_state.onEnter (current_state, initial_event);
		inited = true;
	}

	public bool send_event_immediate(Event ev)
	{
		if (!inited) {
			return false;
		}

		if (update_immediate_nest_level > 0) {
			return send_event_delayed(ev);
		}

		update_immediate_nest_level += 1;

		Transition transition;
		if (!transition_table.transitions [current_state].TryGetValue (ev, out transition))
		{
			update_immediate_nest_level -= 1;
			return false;
		}

		current_state.onExit(current_state, ev);

		onTransition (transition);

		current_state = transition.to;

		current_state.onEnter (current_state, ev);

		update_immediate_nest_level -= 1;

		if (update_immediate_nest_level == 0) {

			while (delayed_events.Count > 0)
			{
				Event ev_delayed = delayed_events.Dequeue ();
				send_event_immediate (ev_delayed);
			}
		}

		return true;
	}

	public bool send_event_delayed(Event ev)
	{
		if (!inited) {
			return false;
		}

		delayed_events.Enqueue (ev);
		return true;
	}

	public bool update()
	{
		if (!inited) {
			return false;
		}

		current_state.onUpdate (current_state);
		this.onUpdate (current_state);
		return true;
	}

	public bool is_inited()
	{
		return inited;
	}

}

