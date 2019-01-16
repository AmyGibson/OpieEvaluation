/*
** Opie Interfaces for Unity
** -----------------------
*/

/* imports */

using UnityEngine;
using System.Net;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;


//! EyeType enum
/*! Eye types that can be selected. */
public enum EyeType
{
	NEUTRAL, /*!< Default eyes.*/
	CONCENTRATED,
	SURPRISED,
	ANGRY,
	SAD,
	ATTENTIVE,
	CUTE,
 	NO_EYE_TYPES
};

//! ArmType enum
/*! Select between left or right arms. */
public enum ArmType
{
	LEFT = 0,
	RIGHT = 1,
 	NO_ARM_TYPES /*!< likely two! */
};

//! BehaviourType
/*! Toggle between switch on, switch off and toggle different behaviours. */
public enum BehaviourType
{
	ON,
	OFF,
	TOGGLE,
	NO_BEHAVIOUR_TYPES
};

//! Internal interface
/*! Provides a setup function for ROS interfaces that is called when ROS is initialized. */
public interface ROSInterface
{
	//! Setup ROS interfaces after ROS has been initialized and connected.
	/*!
	\param node_handle the ROS node handle
	\param topic_root the topic to prepend to any new topics that are registered.
	\sa initializes ROS publishers, subscribers and messages.
	*/
	void setup_ROS_interfaces(ROS.NodeHandle node_handle, string topic_root);
}

//! The Opie class
/*! The main Opie interface class, provides access to Opie's hardware through ROS,
including head, arms and spatial_sensor. Opie is a _singleton_ class (i.e. there is only ever one instance).
*/
public class Opie : ROSInterface
{
    //! the one Opie instance, created when the class is loaded.
    static readonly Opie instance_ = new Opie();

		//! Compile-time flag to enable/disable games
    static readonly bool flag_no_games = false;

		//! Compile-time flag to enable/disable drivers
    static readonly bool flag_no_drivers = false;

		//! Access to Opie's head
    Head head_ = null;

		//! Access to Opie's arms
    List<Arm> arms_ = null;

		//! Access to Opie's tts
    TTS tts_ = null;

		//! Access to Opie's audio
    Audio audio_ = null;

		//! Access to Opie's logging =
    Logging logging_ = null;

		//! Access to Opie's spatial sensor
    SpatialSensor spatial_sensor_ = null;

		//! Access to Opie's game state
    GameControl game_control_ = null;


		//! The game that is currently running
    Game current_game_ = null;

		//! The index of the current game
    uint current_game_index_ = 0;

		//! A set of ROS interfaces that need to be initialized when ROS is initialized
    List<ROSInterface> ros_interfaces_to_setup_ = null;
	List<ROSInterface> ros_interfaces_already_setup_ = null;

    //!list of drivers
		/*! Static so that they can be manipulated _before_ Opie class is instantiated.*/
    static List<Opie.Driver> drivers_ = new List<Opie.Driver>();

		//!list of games
		/*! Static so that they can be manipulated _before_ Opie class is instantiated.*/
    static List<Opie.Game> games_ = new List<Opie.Game>();

		//!list of behaviours
		/*! Static so that they can be manipulated _before_ Opie class is instantiated.*/
    static List<Opie.Behaviour> behaviours_ = new List<Opie.Behaviour>();

    //! ROS node handle
    private ROS.NodeHandle nodeHandle = null;

		//! Name of the node that ROS registers
    private string node_name = null;

		//! A prefix prepended to all topics that Opie registers
    private string topic_root = null;


		//! Update function delegate
    private delegate bool ROSModeUpdate();

		//! The current update function
    private ROSModeUpdate ros_mode_update_;


		//! Static Constructor
		/*! static constructor does not need an ipmlementation, but enforces initialization of
		static variables before other code is called. */
    static Opie()
    {
    }

		//! Access to Opie
    public static Opie instance()
    {
        /* get the singleton instance of Opie */
        return instance_;
    }

		//! Private constructor
    Opie()
    {
        Debug.Log("Opie Constructor");

		// do a check for compatability between ROS.cs and libcs_ros_bridge.so
 		Debug.Log (ROS.print_version ());
		if (!ROS.check_version()) {
			Debug.LogWarning ("Version mismatch! ROS.cs is different version to libcs_ros_bridge.so");
		} 

        // set the initial update function
        ros_mode_update_ = ros_update_not_inited;

        // initialize all of Opie's categories
        head_ = new Head();
        arms_ = new List<Arm>();
        arms_.Add(new Arm(ArmType.LEFT));
        arms_.Add(new Arm(ArmType.RIGHT));
        spatial_sensor_ = new SpatialSensor();
        tts_ = new TTS();
        audio_ = new Audio();
        logging_ = new Logging();

        if (!flag_no_games)
        {
            game_control_ = new GameControl();
        }

        // add each ros interface to a list for initialization
        ros_interfaces_to_setup_ = new List<ROSInterface>();
		ros_interfaces_already_setup_ = new List<ROSInterface> ();
        ros_interfaces_to_setup_.Add(head_);
        foreach (Arm arm in arms_)
        {
            ros_interfaces_to_setup_.Add(arm);
        }
        ros_interfaces_to_setup_.Add(spatial_sensor_);
        ros_interfaces_to_setup_.Add(tts_);
        ros_interfaces_to_setup_.Add(audio_);

        if (!flag_no_games)
        {
            ros_interfaces_to_setup_.Add(game_control_);
        }

        ros_interfaces_to_setup_.Add(logging_);
    }

		//! Setup ROS interfaces for Opie
    public void setup_ROS_interfaces(ROS.NodeHandle node_handle, string unused2)
    {
        /* Function is called once ROS is initialized. Setup all interfaces for
         * all children.
         */
        foreach (ROSInterface ri in ros_interfaces_to_setup_)
        {
            ri.setup_ROS_interfaces(node_handle, topic_root);
			ros_interfaces_already_setup_.Add (ri);
        }

		ros_interfaces_to_setup_.Clear ();

    }

	//! Set node name
	/*!
	Set the name of the node.
	Call before first call to update.
	\param node_name node_name to set (e.g., "opie_torso").
	*/
	public void set_node_name(string node_name)
	{
		if (!ROS.is_inited) {
			this.node_name = node_name;
		}
	}

	//! Set the topic root
	/*!
	Set the topic root for Opie
	Call before first call to update.
	\param topic_root topic root to set with leading slash (e.g., "/opie1")
	*/
	public void set_topic_root(string topic_root)
	{
		if (!ROS.is_inited) {
			this.topic_root = topic_root;
		}
	}

	//! Update function
	/*!
	Call from within a Unity GameObject update, initializes ROS and calls spinOnce.

	update calls one of the following private functions:
	 *
	 * ros_update_not_inited
	 * ros_update_not_connected
	 * post_ros_update_init
	 * ros_update_connected
	 *
	 * which are assigned to ros_mode_update_

	 \return successful update or not
	 */
    public bool update()
    {
        return ros_mode_update_();
    }

    /* Direct access to Opie's sensors, grouped by categories  ------------------------------------------- */

		//! Access to Opie's head
		/*!
		\return Opie.Head object
		*/
    public Head head()
    {
        return head_;
    }

		//! Access to Opie's arms
		/*!
		\param arm_to_get either LEFT or RIGHT arm
		\return Opie.Arm object
		*/
    public Arm arms(ArmType arm_to_get)
    {
			if (arm_to_get < ArmType.NO_ARM_TYPES)
			{
      	return arms_[(int)arm_to_get];
			}
			else
			{
				return null;
			}

    }

		//! Access to Opie's TTS
		/*!
		\return Opie.TTS object
		*/
    public TTS tts()
    {
        return tts_;
    }

		//! Access to Opie's audio
		/*!
		\return Opie.Audio object
		*/
    public Audio audio()
    {
        return audio_;
    }

		//! Access to Opie's spatial sensor
		/*!
		\return Opie.SpatialSensor
		*/
    public SpatialSensor spatial_sensor()
    {
        return spatial_sensor_;
    }

		//! Access to Opie's logging
		/*!
		\return Opie.Logging object
		*/
    public Logging logging()
    {
        return logging_;
    }
    /* ------------------------------------------------------- ------------------------------------------- */

    /* Behaviours API -------------------------------------------------------------------------------------*/

		//! Add a behaviour to the static list of behaviours
		/*!
		\param behaviour Behaviour to add.
		*/
		public static void add_behaviour(Behaviour behaviour)
    {
        behaviours_.Add(behaviour);
    }

		//! Get a list of behaviours.
		/*!
		\return list of behaviours.
		*/
    public List<KeyValuePair<string, int>> list_behaviours()
    {
        Debug.Log("Not implemented yet!");
        return null;
    }


	//! Get a behaviour by name
	/*!
	\param behaviour_to_get name of the behaviour to get
	\return a behaviour
	*/
	public Behaviour behaviours(string behaviour_to_get)
	{
		foreach (Behaviour behaviour in behaviours_) {
			if (behaviour.name () == behaviour_to_get) {
				return behaviour;
			}
		}

		return null; // no matching name found
	}


		//! Get a behaviour by id (might change)
		/*!
		\param behaviour_to_get id of the behaviour to get
		\return a behaviour
		*/
    public Behaviour behaviours(int behaviour_to_get)
    {
		if (behaviour_to_get < behaviours_.Count) {
			return behaviours_ [behaviour_to_get];
		}
		return null;
    }
    /* ----------------------------------------------------------------------------------------------------*/

    /* Games API ------------------------------------------------------------------------------------------*/
		//! Add a game to the static list of games
		/*!
		\param game Opie.Game to add.
		*/
		public static void add_game(Opie.Game game)
    {
        if (flag_no_games)
        {
            // ignore games
            return;
        }

        games_.Add(game);
    }

		//! Get a list of games (on this device)
		/*!
		\return list of games
		*/
    public List<KeyValuePair<string, int>> list_games()
    {
        Debug.Log("Not implemented yet!");
        return null;
    }

		//! Get a game by id (might change)
		/*!
		\param game_to_get id of the Opie.Game to return.
		\return an Opie.Game.
		*/
    public Game games(int game_to_get)
    {
        /* function body */
        Debug.Log("Not implemented yet!");
        return null;
    }

		//! Get the currently executing game (if on this device)
		/*!
		\return Opie.Game.
		*/
    public Game current_game()
    {
        return current_game_;
    }

		//! Get access to game_control.
		/*!
		\return Opie.GameControl.
		*/
    public GameControl game_control()
    {
        return game_control_;
    }
    /* ---------------------------------------------------------------------------------------------------*/

		//! Add a driver to the static list of drivers
		/*!
		\param driver Opie.Driver to add.
		*/
    public static void add_driver(Opie.Driver driver)
    {
        if (flag_no_drivers)
        {
            return; //ignore drivers
        }

        drivers_.Add(driver);
    }

		//! Add a ros interface for Opie to setup when ROS intializes
		/*!
		\param ri a ROSInterface for Opie to store and initialize later.
		*/
    public void add_ros_interface(ROSInterface ri)
    {
        ros_interfaces_to_setup_.Add(ri);
    }


    /* Private functions, not part of interface ------------------------------------------- */

		//! Update function for Opie before ROS is initialized.
		/*!
		\return success or not.
		\sa changes ros_mode_update_ if ROS is initialized.
		*/
    private bool ros_update_not_inited()
    {

        // topic names formed from device identifiers
        // topic_root = "/opie_" + UnityEngine.SystemInfo.deviceUniqueIdentifier;
        // node_name = "opie_" + UnityEngine.SystemInfo.deviceUniqueIdentifier + "_tummy";

		if (topic_root == null) {
			topic_root = "/opie1";
		}

		if (node_name == null) {
			node_name = "opie_part";
		}


        
        if (!ROS.is_inited)
        {
            // get IP address
            IPAddress address = ROS.GetLocalIPAddress();
            string ip = "";
            if (address == null)
            {
                ip = "192.168.0.4"; // default to *.0.4
                Debug.Log("Cannot read my own IP!");
            }
            else
            {
                ip = address.ToString();
            }

            // initialize ROS (does not attempt to connect, does not block).
            ROS.init(ROS.DEFAULT_MASTER_URI, ip, node_name);
        }

        if (!flag_no_drivers) // probably unnecessary, list will be empty anyway
        {
            // initialize drivers
            foreach (Driver driver in drivers_)
            {
                driver.driver_start();
            }
        }


		// add behaviours to the ros interfaces
		foreach (Behaviour behaviour in behaviours_)
		{
			ros_interfaces_to_setup_.Add (behaviour);
		}

        // if ROS is intitialized, we change update mode.
        ros_mode_update_ = ros_update_not_connected;
        return true;
    }

		//! Update function for when ROS is not connected
		/*!
		\return success or not
		\sa changes ros_mode_update_ if ROS becomes connected.
		*/
    private bool ros_update_not_connected()
    {
        /* update function before ROS is connected. */

        // check that ROS is inited and not connected.
        if (ROS.is_inited)
        {
            if (!ROS.is_connected)
            {
                Debug.Log("ROS not connected.");
                if (ROS.NodeHandle.check_master())
                {

                    nodeHandle = new ROS.NodeHandle();

                    if (ROS.is_connected)
                    {
                        setup_ROS_interfaces(nodeHandle, "");
                        Debug.Log("ROS connected.");

                        ros_mode_update_ = post_ros_update_init;

						return true;
                    }
                }
            }
            return false;
        }
        else
        {
            Debug.Log("ROS not inited.");
            return false;
        }
    }

		//! update function once ROS is intitialized
		/*!
		\return success or not
		\sa changes ros_mode_update_
		*/
    private bool post_ros_update_init()
    {
        if (!flag_no_games)
        {
            current_game_index_ = 0;
            GameControlInternal.start_if_exists();
        }

        ros_mode_update_ = ros_update_connected;
        return true;
    }

		//! update function once ROS is connected.
		/*!
		\return success or not
		\sa Executes all ROS callbacks.
		*/
    private bool ros_update_connected()
    {
		if (ros_interfaces_to_setup_.Count > 0) {
			setup_ROS_interfaces (nodeHandle, topic_root);
		}

        /* update function after ROS is connected
         just handle ROS messages*/
        nodeHandle.spin_once();
        return true;
    }
    /* ----------------------------------------------------------------------------------- */

		//! Opie's head
		/*!
		Class that represents Opie's head.
		*/
  public class Head : ROSInterface
	{

        /* class representing Opie's head. Includes gimbal motion and eye motion */
		private Vector3 current_pose_;
		private Vector3 reference_pose_;
		private Vector2 eye_position_;
        private EyeType eye_type_;
        private BehaviourType blinking_;

		private ROS.Publisher headMovementPub = null;
		private IntPtr headMovementMessage = IntPtr.Zero;

		private ROS.Publisher emotionActionPub = null;
		private IntPtr emotionActionMessage = IntPtr.Zero;

		private ROS.Publisher eyeMovementPub = null;
		private IntPtr eyeMovementMessage = IntPtr.Zero;

		private ROS.Publisher headDeltaMovementPub = null;
		private IntPtr headDeltaMovementMessage = IntPtr.Zero;

        public struct CallbackData
        {
            Vector3 pose;
        }

		public void set_linked_pose_and_eye_position(float x, float y, TransitionAction<CallbackData> action)
		{
			set_pose(new Vector3(reference_pose_.x, reference_pose_.y + (0.5f - y)*30, reference_pose_.z+(x-0.5f)*30), action);
			set_eye_position(x, y, action);
		}

	    public void set_pose(Vector3 pose, TransitionAction<CallbackData> action)
	    {
		    IntPtr payload2 = ROS.cs_ros_message_Vector3Stamped_get_pointer(headMovementMessage);
		    IntPtr payloadvector = ROS.cs_ros_message_Vector3Stamped_get_pointer_vector(payload2);
		    ROS.cs_ros_message_Vector3_set_x(payloadvector, pose.x);
		    ROS.cs_ros_message_Vector3_set_y(payloadvector, pose.y);
		    ROS.cs_ros_message_Vector3_set_z(payloadvector, pose.z);
		    IntPtr header2 = ROS.cs_ros_message_Vector3Stamped_get_pointer_header(payload2);
		    ROS.cs_ros_message_Header_set_seq(header2, ROS.cs_ros_message_Header_get_seq(header2) + 1);
		    headMovementPub.publish(headMovementMessage);
	    }

        public void set_delta_pose(Vector3 delta_pose, TransitionAction<CallbackData> action)
        {
	        IntPtr payload = ROS.cs_ros_message_Vector3Stamped_get_pointer(headDeltaMovementMessage);
	        IntPtr vector = ROS.cs_ros_message_Vector3Stamped_get_pointer_vector(payload);
	        ROS.cs_ros_message_Vector3_set_x(vector, delta_pose.x);
	        ROS.cs_ros_message_Vector3_set_y(vector, delta_pose.y);
	        ROS.cs_ros_message_Vector3_set_z(vector, delta_pose.z);
	        IntPtr header = ROS.cs_ros_message_Vector3Stamped_get_pointer_header(payload);
	        ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
	        headDeltaMovementPub.publish(headDeltaMovementMessage);
        }

	    public Vector3 pose()
	    {
	        return current_pose_;
	    }

	    public void set_eye_position(float x, float y, TransitionAction<CallbackData> action)
	    {
	        IntPtr payload = ROS.cs_ros_message_EyeMovement_get_pointer(eyeMovementMessage);
	        ROS.cs_ros_message_EyeMovement_set_x(payload, x);
	        ROS.cs_ros_message_EyeMovement_set_y(payload, y);
	        IntPtr header = ROS.cs_ros_message_EyeMovement_get_pointer_header(payload);
	        ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
	        eyeMovementPub.publish(eyeMovementMessage);
            eye_position_.x = x;
            eye_position_.y = y;
        }

	    public Vector2 eye_position()
	    {
		    return eye_position_;
	    }

	    public void set_eye_type(EyeType eye_type_to_use, InstantAction action)
	    {
            IntPtr payload = ROS.cs_ros_message_EmotionAction_get_pointer(emotionActionMessage);
            ROS.cs_ros_message_EmotionAction_set_emotion_action(payload, (uint)eye_type_to_use);

            IntPtr header = ROS.cs_ros_message_EmotionAction_get_pointer_header(payload);
            ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
            ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));

            emotionActionPub.publish(emotionActionMessage);

            eye_type_ = eye_type_to_use;
	    }

	    public EyeType eye_type()
	    {
	        return eye_type_;
	    }

        public void set_blinking(BehaviourType blinking)
        {
            blinking_ = blinking;
        }

	    public BehaviourType blinking()
	    {
	        return blinking_;
	    }

        public static InstantAction instant_action()
        {
            return new InstantAction();
        }

        public static TransitionAction<CallbackData> transition_action()
        {
            return new TransitionAction<CallbackData>();
        }

		void ROSInterface.setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
		{
            emotionActionPub = nodeHandle.advertise(topic_root + "/head/eyes/emotion", 1,
													ROS.cs_ros_message_EmotionAction_publisher_ctor);
			emotionActionMessage = ROS.cs_ros_message_EmotionAction_create();

			eyeMovementPub = nodeHandle.advertise(topic_root + "/head/eyes/movement", 1,
							ROS.cs_ros_message_EyeMovement_publisher_ctor);
			eyeMovementMessage = ROS.cs_ros_message_EyeMovement_create();

			headMovementPub=nodeHandle.advertise(topic_root + "/head/gimbal",1,ROS.cs_ros_message_Vector3Stamped_publisher_ctor);
			headMovementMessage= ROS.cs_ros_message_Vector3Stamped_create();
		}
	}

	//! Opie Arm
	/*!
 	* Class that represents one of Opie's arms. Contains functions to set joint positions, and to receive motions.
	*/
	public class Arm : ROSInterface
	{
        ArmType arm_type_;

        ROS.Publisher elbowPublisher = null;
        ROS.Publisher shoulderPublisher = null;
		ROS.Publisher elbowAnglePublisher = null;
        ROS.Publisher shoulderAnglePublisher = null;
		ROS.Publisher medialRotationAnglePublisher = null;

		ROS.Subscriber elbowAngleSubscriber = null;
		ROS.Subscriber shoulderAngleSubscriber = null;
		ROS.Subscriber medialRotationAngleSubscriber = null;

		ROS.Subscriber elbowProgressSubscriber = null;
		ROS.Subscriber shoulderProgressSubscriber = null;
		ROS.Subscriber medialRotationProgressSubscriber = null;

        IntPtr elbowMessage = IntPtr.Zero;
        IntPtr shoulderMessage = IntPtr.Zero;
        IntPtr medialRotationMessage = IntPtr.Zero;

		IntPtr elbowAngleMessage = IntPtr.Zero;
		IntPtr shoulderAngleMessage = IntPtr.Zero;
		IntPtr medialRotationAngleMessage = IntPtr.Zero;


		public struct CallbackData
		{
			public uint status;
			public float progress;
		}

		uint sequence_ = 0;
		private Dictionary<uint, TransitionAction<CallbackData> > callbacks_ = new Dictionary<uint, TransitionAction<CallbackData> > ();



        public Arm(ArmType arm_type)
        {
            arm_type_ = arm_type;
        }

	    public List<double> joint_positions()
	    {
	        Debug.Log("Not implemented yet!");
		    return new List<double>();
	    }

	    public void set_hand_position(Vector3 position_relative_to_opie, TransitionAction<CallbackData> action)
	    {
	        Debug.Log("Not implemented yet!");
	    }

	    public Vector3 hand_position()
	    {
	        Debug.Log("Not implemented yet!");
		    return new Vector3();
	    }

	    public void set_led_transitions(List<Color> colors, TransitionAction<CallbackData> action)
	    {
	        Debug.Log("Not implemented yet!");
	    }

        public void set_elbow_counts(uint counts, TransitionAction<CallbackData> action)
        {
            IntPtr payload = ROS.cs_ros_message_stepper_params_get_pointer(elbowMessage);

            ROS.cs_ros_message_stepper_params_set_index(payload, 0); // seems like index can only be 0 for now
            ROS.cs_ros_message_stepper_params_set_engage(payload, 1); // need to engage the motor in order to move
            ROS.cs_ros_message_stepper_params_set_acceleration(payload, 600.0f); // acceleration _LIMIT_ (in counts/s/s?)
            ROS.cs_ros_message_stepper_params_set_velocity(payload, 1000.0f); // velocity _LIMIT_ (in counts/s?)
            ROS.cs_ros_message_stepper_params_set_reset_position(payload, 1); // puts the motor into a relative mode
            ROS.cs_ros_message_stepper_params_set_position(payload, counts); // the position of the motor (3000 counts = 360d for the shoulder)

            IntPtr header = ROS.cs_ros_message_stepper_params_get_pointer_header(payload);
            ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
            ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));

            elbowPublisher.publish(elbowMessage);

        }

		public void set_elbow_angle(float angle, TransitionAction<CallbackData> action)
		{
			// set some defaults
			set_elbow_angle(angle, true, 0.0f, 0.0f, 0.0f, action);
		}

		public void set_elbow_angle(float angle, bool engage, float velocity_limit, float acceleration_limit, float current_limit, TransitionAction<CallbackData> action)
		{
			IntPtr payload = ROS.cs_ros_message_MotorPosition_get_pointer(elbowAngleMessage);

			ROS.cs_ros_message_MotorPosition_set_engage(payload, (byte)(engage ? 1 : 0));


			/*ROS.cs_ros_message_MotorPosition_set_velocity_limit(payload, velocity_limit);
			ROS.cs_ros_message_MotorPosition_set_acceleration_limit(payload, acceleration_limit);
			ROS.cs_ros_message_MotorPosition_set_current_limit(payload, current_limit);*/

			ROS.cs_ros_message_MotorPosition_set_position_deg(payload, angle);

			IntPtr header = ROS.cs_ros_message_MotorPosition_get_pointer_header(payload);
			ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
			ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));


			sequence_ += 1;
			IntPtr transition_action = ROS.cs_ros_message_MotorPosition_get_pointer_action (payload);

			IntPtr action_ref = ROS.cs_ros_message_TransitionAction_get_pointer_ref (transition_action);


			ROS.cs_ros_message_ActionRef_set_seq (action_ref, sequence_);

			ROS.cs_ros_string_set (ROS.cs_ros_message_ActionRef_get_pointer_sender (action_ref), Opie.instance().node_name);

			callbacks_.Add (sequence_, action);

			elbowAnglePublisher.publish(elbowAngleMessage);
		}

        public void set_shoulder_counts(uint counts, TransitionAction<CallbackData> action)
        {
            IntPtr payload = ROS.cs_ros_message_stepper_params_get_pointer(shoulderMessage);

            ROS.cs_ros_message_stepper_params_set_index(payload, 0); // seems like index can only be 0 for now
            ROS.cs_ros_message_stepper_params_set_engage(payload, 1); // need to engage the motor in order to move
            ROS.cs_ros_message_stepper_params_set_acceleration(payload, 600.0f); // acceleration _LIMIT_ (in counts/s/s?)
            ROS.cs_ros_message_stepper_params_set_velocity(payload, 1000.0f); // velocity _LIMIT_ (in counts/s?)
            ROS.cs_ros_message_stepper_params_set_reset_position(payload, 1); // puts the motor into a relative mode
            ROS.cs_ros_message_stepper_params_set_position(payload, counts); // the position of the motor (3000 counts = 360d for the shoulder)

            IntPtr header = ROS.cs_ros_message_stepper_params_get_pointer_header(payload);
            ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
            ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));

            shoulderPublisher.publish(shoulderMessage);
        }

		public void set_shoulder_angle(float angle, TransitionAction<CallbackData> action)
		{
			// set some defaults
			set_shoulder_angle(angle, true, 0.0f, 0.0f, 0.0f, action);
		}

		public void set_shoulder_angle(float angle, bool engage, float velocity_limit, float acceleration_limit, float current_limit, TransitionAction<CallbackData> action)
		{
			IntPtr payload = ROS.cs_ros_message_MotorPosition_get_pointer(shoulderAngleMessage);

			ROS.cs_ros_message_MotorPosition_set_engage(payload, (byte)(engage ? 1 : 0));


			// do not touch these yet, they need reasonable limits
			/*ROS.cs_ros_message_MotorPosition_set_velocity_limit(payload, velocity_limit);
			ROS.cs_ros_message_MotorPosition_set_acceleration_limit(payload, acceleration_limit);
			ROS.cs_ros_message_MotorPosition_set_current_limit(payload, current_limit);*/

			ROS.cs_ros_message_MotorPosition_set_position_deg(payload, angle);

			IntPtr header = ROS.cs_ros_message_MotorPosition_get_pointer_header(payload);
			ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
			ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));

			sequence_ += 1;
			IntPtr transition_action = ROS.cs_ros_message_MotorPosition_get_pointer_action (payload);

			IntPtr action_ref = ROS.cs_ros_message_TransitionAction_get_pointer_ref (transition_action);


			ROS.cs_ros_message_ActionRef_set_seq (action_ref, sequence_);

			ROS.cs_ros_string_set (ROS.cs_ros_message_ActionRef_get_pointer_sender (action_ref), Opie.instance().node_name);

			callbacks_.Add (sequence_, action);

			shoulderAnglePublisher.publish(shoulderAngleMessage);
		}

		public void set_medial_rotation_counts(uint counts, TransitionAction<CallbackData> action)
		{
			IntPtr payload = ROS.cs_ros_message_stepper_params_get_pointer(medialRotationMessage);

			ROS.cs_ros_message_stepper_params_set_index(payload, 0); // seems like index can only be 0 for now
			ROS.cs_ros_message_stepper_params_set_engage(payload, 1); // need to engage the motor in order to move
			ROS.cs_ros_message_stepper_params_set_acceleration(payload, 600.0f); // acceleration _LIMIT_ (in counts/s/s?)
			ROS.cs_ros_message_stepper_params_set_velocity(payload, 1000.0f); // velocity _LIMIT_ (in counts/s?)
			ROS.cs_ros_message_stepper_params_set_reset_position(payload, 1); // puts the motor into a relative mode
			ROS.cs_ros_message_stepper_params_set_position(payload, counts); // the position of the motor (3000 counts = 360d for the shoulder)

			IntPtr header = ROS.cs_ros_message_stepper_params_get_pointer_header(payload);
			ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
			ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));

			shoulderPublisher.publish(medialRotationMessage);
		}

		public void set_medial_rotation_angle(float angle, TransitionAction<CallbackData> action)
		{
			// set some defaults
			set_medial_rotation_angle(angle, true, 0.0f, 0.0f, 0.0f, action);
		}

		public void set_medial_rotation_angle(float angle, bool engage, float velocity_limit, float acceleration_limit, float current_limit, TransitionAction<CallbackData> action)
		{
			IntPtr payload = ROS.cs_ros_message_MotorPosition_get_pointer(medialRotationAngleMessage);

			ROS.cs_ros_message_MotorPosition_set_engage(payload, (byte) (engage ? 1 : 0));


			/* ROS.cs_ros_message_MotorPosition_set_velocity_limit(payload, velocity_limit);
			ROS.cs_ros_message_MotorPosition_set_acceleration_limit(payload, acceleration_limit);
			ROS.cs_ros_message_MotorPosition_set_current_limit(payload, current_limit); */

			ROS.cs_ros_message_MotorPosition_set_position_deg(payload, angle);

			IntPtr header = ROS.cs_ros_message_MotorPosition_get_pointer_header(payload);
			ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);
			ROS.cs_ros_time_set_now(ROS.cs_ros_message_Header_get_pointer_stamp(header));

			sequence_ += 1;

			IntPtr action_ref = ROS.cs_ros_message_MotorPosition_get_pointer_action (payload);
			ROS.cs_ros_message_ActionRef_set_seq (action_ref, sequence_);
			ROS.cs_ros_string_set (ROS.cs_ros_message_ActionRef_get_pointer_sender (action_ref), Opie.instance ().node_name);

			callbacks_.Add (sequence_, action);

			medialRotationAnglePublisher.publish(medialRotationAngleMessage);
		}

        public static InstantAction instant_action()
        {
            return new InstantAction();
        }


        public static TransitionAction<CallbackData> transition_action()
        {
            return new TransitionAction<CallbackData>();
        }

		void progress_callback(IntPtr message)
		{
			IntPtr action_update = ROS.cs_ros_message_ActionUpdate_get_pointer (message);

			IntPtr action_ref = ROS.cs_ros_message_ActionUpdate_get_pointer_ref (action_update);

			string sender = ROS.cs_ros_string_get_as_string (ROS.cs_ros_message_ActionRef_get_pointer_sender (action_ref));

			// Debug.Log ("sender: " + sender);

			if (sender == Opie.instance().node_name) {

				uint seq = ROS.cs_ros_message_ActionRef_get_seq (action_ref);
				uint status = ROS.cs_ros_message_ActionUpdate_get_status (action_update);
				float progress = ROS.cs_ros_message_ActionUpdate_get_progress (action_update);

				/*Debug.Log ("callback: " + seq);

				foreach (KeyValuePair<uint, TransitionAction<CallbackData> > pair in callbacks_)
				{
					Debug.Log("key: " + pair.Key + ", value: "+ pair.Value);
				}*/

				// Debug.Log ("got callback");

				TransitionAction<CallbackData> ta;

				if (callbacks_.TryGetValue (seq, out ta)) {

					// Debug.Log ("found key");
					CallbackData cd;
					cd.status = status;
					cd.progress = progress;

					(ta.status_callback()) (cd);
				}

				if (status != Action.IN_PROGRESS) {
					callbacks_.Remove (seq);
				}
			}
		}

        void ROSInterface.setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
        {
            // ArmType will need to be included in topic. i.e. topic_root+"/arms/" + (arm_type_== LEFT ? "left" : "right") + "/*";
            elbowPublisher = nodeHandle.advertise(topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/elbow/stepper_command",
                1, ROS.cs_ros_message_stepper_params_publisher_ctor);
            shoulderPublisher = nodeHandle.advertise(topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/shoulder/stepper_command",
                1, ROS.cs_ros_message_stepper_params_publisher_ctor);

			elbowAnglePublisher = nodeHandle.advertise(topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/elbow/stepper_command_angle",
					1, ROS.cs_ros_message_MotorPosition_publisher_ctor);
			shoulderAnglePublisher = nodeHandle.advertise(topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/shoulder/stepper_command_angle",
					1, ROS.cs_ros_message_MotorPosition_publisher_ctor);
			medialRotationAnglePublisher = nodeHandle.advertise(topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/medial/stepper_command_angle",
					1, ROS.cs_ros_message_MotorPosition_publisher_ctor);

			/*
			elbowAngleSubscriber = nodeHandle.subscribe (topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/elbow/stepper_value",
				1, elbow_angle_callback_handle, ROS.cs_ros_message_encoder_params_subscriber_ctor);
			shoulderAngleSubscriber = nodeHandle.subscribe (topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/elbow/stepper_value",
				1, shoulder_angle_callback_handle, ROS.cs_ros_message_encoder_params_subscriber_ctor);
			medialRotationAngleSubscriber = nodeHandle.subscribe (topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/elbow/stepper_value",
				1, medial_rotation_angle_callback_handle, ROS.cs_ros_message_encoder_params_subscriber_ctor);
			*/

			ROS.MessageCallback elbow_progress_callback_handle = new ROS.MessageCallback((IntPtr message) => this.progress_callback(message));
			GCHandle.Alloc(elbow_progress_callback_handle);
			elbowProgressSubscriber = nodeHandle.subscribe (topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/elbow/progress",
				1, elbow_progress_callback_handle, ROS.cs_ros_message_ActionUpdate_subscriber_ctor);

			ROS.MessageCallback shoulder_progress_callback_handle = new ROS.MessageCallback((IntPtr message) => this.progress_callback(message));
			GCHandle.Alloc(shoulder_progress_callback_handle);
			shoulderProgressSubscriber = nodeHandle.subscribe (topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/shoulder/progress",
				1, shoulder_progress_callback_handle, ROS.cs_ros_message_ActionUpdate_subscriber_ctor);

			ROS.MessageCallback medial_progress_callback_handle = new ROS.MessageCallback((IntPtr message) => this.progress_callback(message));
			GCHandle.Alloc(medial_progress_callback_handle);
			medialRotationProgressSubscriber = nodeHandle.subscribe (topic_root + "/arms/" + (arm_type_ == ArmType.LEFT ? "left" : "right") + "/medial/progress",
				1, medial_progress_callback_handle, ROS.cs_ros_message_ActionUpdate_subscriber_ctor);

			elbowMessage = ROS.cs_ros_message_stepper_params_create();
			shoulderMessage = ROS.cs_ros_message_stepper_params_create();
			medialRotationMessage = ROS.cs_ros_message_stepper_params_create();

			elbowAngleMessage = ROS.cs_ros_message_MotorPosition_create();
			shoulderAngleMessage = ROS.cs_ros_message_MotorPosition_create();
			medialRotationAngleMessage = ROS.cs_ros_message_MotorPosition_create();
			Debug.Log ("initialized arm");

        }
	}

		//! Class that represents Opie's TTS interface
    public class TTS : ROSInterface
    {
        /* Class represents Opie's TTS abilities */

	    private ROS.Publisher ttsPub = null;
	    private ROS.Subscriber ttsSub = null;

        uint reference_id = 0; // the current id for tts messages

        public struct CallbackData
        {
            public string text;
            public uint id;
        }

        private Dictionary<uint, UntimedAction<CallbackData> > callbacks_;

        public TTS()
        {
            callbacks_ = new Dictionary<uint, UntimedAction<CallbackData>>();
        }

        public void setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
        {
		    ttsPub =  nodeHandle.advertise(topic_root + "/tts/speak", 0,
			    ROS.cs_ros_message_TTS_publisher_ctor);

		    ROS.MessageCallback done_callback_handle = new ROS.MessageCallback((IntPtr message) => this.done_callback(message));
		    GCHandle.Alloc(done_callback_handle);
		    ttsSub = nodeHandle.subscribe(topic_root + "/tts/done", 0, done_callback_handle,
                ROS.cs_ros_message_TTSStop_subscriber_ctor);
        }

        public void speak(string text_to_speak, UntimedAction<CallbackData> action)
        {
            Debug.Log("speak");
            Debug.Log(text_to_speak);

            IntPtr new_tts_message = ROS.cs_ros_message_TTS_create(); // need to create message here, as we might be sending intraprocess

            IntPtr payload = ROS.cs_ros_message_TTS_get_pointer(new_tts_message);
						IntPtr text = ROS.cs_ros_message_TTS_get_pointer_text(payload);
						ROS.cs_ros_string_set(text, text_to_speak);

						ROS.cs_ros_message_TTS_set_action_type(payload, ROS.cs_ros_message_TTS_get_SPEAK());

						IntPtr header = ROS.cs_ros_message_TTS_get_pointer_header(payload);

            uint seq = ROS.cs_ros_message_Header_get_seq(header) + 1;
            ROS.cs_ros_message_Header_set_seq(header, seq);

            uint ref1 = ++reference_id;
            ROS.cs_ros_message_TTS_set_ref(payload, ref1);

            IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
            ROS.cs_ros_time_set_now(time_stamp);

            callbacks_.Add(ref1, action);
            ttsPub.publish(new_tts_message);

            ROS.cs_ros_message_TTS_destroy(new_tts_message); // safe to destroy the message here.
        }

				public void stop(UntimedAction<CallbackData> action)
				{
					IntPtr new_tts_message = ROS.cs_ros_message_TTS_create(); // need to create message here, as we might be sending intraprocess

					IntPtr payload = ROS.cs_ros_message_TTS_get_pointer(new_tts_message);
					IntPtr text = ROS.cs_ros_message_TTS_get_pointer_text(payload);
					ROS.cs_ros_string_set(text, "");

					ROS.cs_ros_message_TTS_set_action_type(payload, ROS.cs_ros_message_TTS_get_STOP());

					IntPtr header = ROS.cs_ros_message_TTS_get_pointer_header(payload);

					uint seq = ROS.cs_ros_message_Header_get_seq(header) + 1;
					ROS.cs_ros_message_Header_set_seq(header, seq);

					uint ref1 = ++reference_id;
					ROS.cs_ros_message_TTS_set_ref(payload, ref1);

					IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
					ROS.cs_ros_time_set_now(time_stamp);

					callbacks_.Add(ref1, action);
					ttsPub.publish(new_tts_message);

					ROS.cs_ros_message_TTS_destroy(new_tts_message); // safe to destroy the message here.
				}

		public void done_callback(IntPtr message)
		{
			// get seq from return message. TODO: change to id
			IntPtr payload = ROS.cs_ros_message_TTSStop_get_pointer(message);
			IntPtr header = ROS.cs_ros_message_TTSStop_get_pointer_header(payload);
            uint ref1 = ROS.cs_ros_message_TTSStop_get_tts_ref(payload);

            IntPtr text = ROS.cs_ros_message_TTSStop_get_pointer_text(payload);


            // find appropriate callbacks
            UntimedAction<CallbackData> action = null;
            if (callbacks_.TryGetValue(ref1, out action))
            {
                CallbackData cd;
                cd.text = ROS.cs_ros_string_get_as_string(text);
                cd.id = ref1;

                callbacks_.Remove(ref1); // don't let this be called twice, as is the case with two responders.

                Debug.Log("triggering callback");
                // call. TODO: set argument
                (action.status_callback())(cd);
            }

		}

        public static UntimedAction<CallbackData> untimed_action()
        {
            return new UntimedAction<CallbackData>();
        }
    }

		//! Class that represents Opie's audio interface
		/*! Play audio clips */
    public class Audio : ROSInterface
    {
        /* Class to play audio */

        private ROS.Publisher audioPub = null;
        private ROS.Subscriber audioSub = null;

        uint reference_id = 0; // give audio messages that leave this class with an id

        public struct CallbackData
        {
            public string clip_id;
            public uint id;
        }

        private Dictionary<uint, UntimedAction<CallbackData>> callbacks_;

        public Audio()
        {
            callbacks_ = new Dictionary<uint, UntimedAction<CallbackData>>();
        }

        public void setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
        {
            audioPub = nodeHandle.advertise(topic_root + "/audio/play", 0,
                ROS.cs_ros_message_TTS_publisher_ctor);

            ROS.MessageCallback done_callback_handle = new ROS.MessageCallback((IntPtr message) => this.done_callback(message));
            GCHandle.Alloc(done_callback_handle);
            audioSub = nodeHandle.subscribe(topic_root + "/audio/done", 0, done_callback_handle,
                ROS.cs_ros_message_TTSStop_subscriber_ctor);
        }

        public void play(string clip_to_play, UntimedAction<CallbackData> action)
        {
            Debug.Log("play");
            Debug.Log(clip_to_play);
			IntPtr new_audio_message = ROS.cs_ros_message_TTS_create(); // need to create a new messsage here as we might be sending intraprocess

            IntPtr payload = ROS.cs_ros_message_TTS_get_pointer(new_audio_message);
            IntPtr text = ROS.cs_ros_message_TTS_get_pointer_text(payload);
            ROS.cs_ros_string_set(text, clip_to_play);

            IntPtr header = ROS.cs_ros_message_TTS_get_pointer_header(payload);

            uint seq = ROS.cs_ros_message_Header_get_seq(header) + 1;
            ROS.cs_ros_message_Header_set_seq(header, seq);

            //uint ref1 = ROS.cs_ros_message_TTS_get_ref(payload) + 1;
            uint ref1 = ++reference_id;
            ROS.cs_ros_message_TTS_set_ref(payload, ref1);

            IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
            ROS.cs_ros_time_set_now(time_stamp);

            callbacks_.Add(ref1, action);
            audioPub.publish(new_audio_message);

			// safe to destroy here (shared_ptr part will live on)
			ROS.cs_ros_message_TTS_destroy(new_audio_message);
        }

        public void done_callback(IntPtr message)
        {
            // get seq from return message. TODO: change to id
            IntPtr payload = ROS.cs_ros_message_TTSStop_get_pointer(message);
            IntPtr header = ROS.cs_ros_message_TTSStop_get_pointer_header(payload);
            uint ref1 = ROS.cs_ros_message_TTSStop_get_tts_ref(payload);

            IntPtr text = ROS.cs_ros_message_TTSStop_get_pointer_text(payload);


            // find appropriate callbacks
            UntimedAction<CallbackData> action = null;
            if (callbacks_.TryGetValue(ref1, out action))
            {
                CallbackData cd;
                cd.clip_id = ROS.cs_ros_string_get_as_string(text);
                cd.id = ref1;

				callbacks_.Remove(ref1); // remove callback before executing

                Debug.Log("triggering audio callback");
                // call. TODO: set argument
                (action.status_callback())(cd);
            }

        }

        public static UntimedAction<CallbackData> untimed_action()
        {
            return new UntimedAction<CallbackData>();
        }
    }

		//! Class that represents Opie's spatial sensor
    public class SpatialSensor : ROSInterface
	{
        /* Class represents Opie's spatial sensor (likely attached to Opie's big brain) */
		delegate void AudioDirectionUpdate(double audio_direction);

	    void add_audio_direction_listener(AudioDirectionUpdate called_on_audio_direction_update)
	    {

	    }

		void ROSInterface.setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
        {

		}

	}

	//! Access to a behaviour for Opie.
	public class Behaviour : ROSInterface
	{
        /* General behaviour class */

        string name_ = null;

		ROS.Publisher behaviourControlPub = null;
		IntPtr behaviourControlMessage = IntPtr.Zero;

        public Behaviour(string name)
        {
            name_ = name;
        }

		public string name()
		{
			return name_;
		}

	    public void start(InstantAction action)
	    {
			IntPtr payload = ROS.cs_ros_message_BehaviourControl_get_pointer(behaviourControlMessage);
			ROS.cs_ros_message_BehaviourControl_set_action(payload, ROS.cs_ros_message_BehaviourControl_get_START());
			behaviourControlPub.publish (behaviourControlMessage);
	    }

	    public void stop(InstantAction action)
	    {
			IntPtr payload = ROS.cs_ros_message_BehaviourControl_get_pointer(behaviourControlMessage);
			ROS.cs_ros_message_BehaviourControl_set_action(payload, ROS.cs_ros_message_BehaviourControl_get_STOP());
			behaviourControlPub.publish (behaviourControlMessage);
	    }

		public void pause(InstantAction action)
	    {
			IntPtr payload = ROS.cs_ros_message_BehaviourControl_get_pointer(behaviourControlMessage);
			ROS.cs_ros_message_BehaviourControl_set_action(payload, ROS.cs_ros_message_BehaviourControl_get_PAUSE());
			behaviourControlPub.publish (behaviourControlMessage);
	    }

		public void toggle(InstantAction action)
		{
			IntPtr payload = ROS.cs_ros_message_BehaviourControl_get_pointer(behaviourControlMessage);
			ROS.cs_ros_message_BehaviourControl_set_action(payload, ROS.cs_ros_message_BehaviourControl_get_TOGGLE());
			behaviourControlPub.publish (behaviourControlMessage);
		}

		public void setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
        {
            /* Use name in topic (e.g. topic_root + "/behaviours/" + name_ ) */
			behaviourControlPub = nodeHandle.advertise(topic_root + "/behaviours/" + name_, 0,
				ROS.cs_ros_message_BehaviourControl_publisher_ctor);
			behaviourControlMessage = ROS.cs_ros_message_BehaviourControl_create();

        }

		public static InstantAction instant_action()
		{
			return new InstantAction();
		}
	}

	//! Game interface
	/*!
	An Opie game (Abstract)
	Games should provide a class that inherits from this class and
	runs the Game.
	*/
    public abstract class Game
    {
        /* An Opie game (Abstract)
         * Games should provide a class that inherits from this class and
         * runs the Game.
         *  */

        public abstract string game_name();

        // start/stop
        public abstract void game_start();
        public abstract void game_stop();

        // pause and resume
        public abstract void game_pause();
        public abstract void game_resume();

        // games can override next and previous. Default is to defer to next/previous game.
        public virtual bool game_next()
        {
            return false;
        }

        public virtual bool game_previous()
        {
            return false;
        }

    }

		//! Represents the interface for an Opie driver.
		/*!
		Drivers should inherit from this class.
		*/
    public abstract class Driver : ROSInterface
    {
        public abstract string driver_name();
        public abstract void driver_start();
        public abstract void driver_stop();
        public abstract void setup_ROS_interfaces(ROS.NodeHandle n, string s);
    }

		//! Internal interfaces for stopping and starting games
    private class GameControlInternal
    {
        public static bool start_if_exists()
        {
            if (Opie.instance().current_game_ != null || Opie.instance().current_game_index_ >= Opie.games_.Count)
            {
                return false;
            }
            else
            {
                Opie.instance().current_game_ = Opie.games_[(int)Opie.instance().current_game_index_];
                Opie.instance().current_game_.game_start();
                return true;
            }
        }

        public static bool stop_if_exists()
        {
            if (Opie.instance().current_game_ == null)
            {
                return false;
            }
            else
            {
                Opie.instance().current_game_.game_stop();
                Opie.instance().current_game_ = null;
                return true;
            }
        }

        public static bool pause_if_exists()
        {
            if (Opie.instance().current_game_ == null)
            {
                return false;
            }
            else
            {
                Opie.instance().current_game_.game_pause();
                return true;
            }
        }

        public static bool resume_if_exists()
        {
            if (Opie.instance().current_game_ == null)
            {
                return false;
            }
            else
            {
                Opie.instance().current_game_.game_resume();
                return true;
            }
        }

        public static bool next_if_exists()
        {
            if (Opie.instance().current_game_index_ + 1 >= Opie.games_.Count)
            {
                return false;
            }
            else
            {
                Opie.instance().current_game_index_ += 1;
                return start_if_exists();
            }
        }

        public static bool previous_if_exists()
        {
            if (Opie.instance().current_game_index_ == 0)
            {
                return false;
            }
            else
            {
                Opie.instance().current_game_index_ -= 1;
                return start_if_exists();
            }
        }
    }

		//! Public interface for game control.
	public class GameControl : ROSInterface
	{

		private ROS.Subscriber gameControlSub = null;

        delegate void GamePausedCallback();
        delegate void GameExitingCallback();


				//! Method to call when a game is finished.
				/*!
				\sa Stops the current game by calling it's game_stop method, then
				starts the next game in the list by calling it's game_start method.
				*/
        public void finished()
        {
            // stop the current game
            GameControlInternal.stop_if_exists();
            GameControlInternal.next_if_exists();
        }

        void game_control_callback(IntPtr message)
        {
            IntPtr payload = ROS.cs_ros_message_GameControl_get_pointer(message);
            uint command = ROS.cs_ros_message_GameControl_get_command(payload);
            string name = ROS.cs_ros_string_get_as_string(ROS.cs_ros_message_GameControl_get_pointer_name(payload));

            Debug.Log("Received game control command = " + command);

            if (command == ROS.cs_ros_message_GameControl_get_EXIT())
            {
                GameControlInternal.stop_if_exists();
            }
            else if (command == ROS.cs_ros_message_GameControl_get_NEXT())
            {
                if (Opie.instance().current_game_ == null ||  !Opie.instance().current_game_.game_next())
                {
                    GameControlInternal.stop_if_exists();
                    GameControlInternal.next_if_exists();
                }
            }
            else if (command == ROS.cs_ros_message_GameControl_get_PREVIOUS())
            {
                if (Opie.instance().current_game_ == null || !Opie.instance().current_game_.game_previous())
                {
                    GameControlInternal.stop_if_exists();
                    GameControlInternal.previous_if_exists();
                }
            }
            else if (command == ROS.cs_ros_message_GameControl_get_RESTART())
            {
                GameControlInternal.stop_if_exists();
                GameControlInternal.start_if_exists();
            }
            else if (command == ROS.cs_ros_message_GameControl_get_PAUSE())
            {
                GameControlInternal.pause_if_exists();
            }
            else if (command == ROS.cs_ros_message_GameControl_get_RESUME())
            {
                GameControlInternal.resume_if_exists();
            }
            else if (command == ROS.cs_ros_message_GameControl_get_START())
            {
                if (name == "")
                {
                    // no name given, start current game
                    GameControlInternal.start_if_exists();
                }
                else
                {
                    // name given, stop current game, and run given name
                    GameControlInternal.stop_if_exists();

                    // find game by name
                    for (int i = 0; i < Opie.games_.Count; i++)
                    {
                        Game game = Opie.games_[i];
                        if (game.game_name() == name)
                        {
                            Opie.instance().current_game_index_ = (uint) i;
                            break;
                        }
                    }

                    // run that game
                    GameControlInternal.start_if_exists();
                }
            }
        }

	    void ROSInterface.setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
        {
		    ROS.MessageCallback game_control_callback_handle = new ROS.MessageCallback(game_control_callback);
		    GCHandle.Alloc(game_control_callback_handle);

		    gameControlSub = nodeHandle.subscribe(topic_root + "/game/control", 0,
					    game_control_callback_handle,
					    ROS.cs_ros_message_GameControl_subscriber_ctor);
	    }
	}

	//! A generic action
	public class Action
	{
		public static uint IN_PROGRESS = ROS.cs_ros_message_ActionUpdate_get_IN_PROGRESS ();
		public static uint COMPLETED_SUCCESSFULLY = ROS.cs_ros_message_ActionUpdate_get_COMPLETED_SUCCESSFULLY ();
		public static uint COMPLETED_FAILURE = ROS.cs_ros_message_ActionUpdate_get_COMPLETED_FAILURE ();
		public static uint INTERRUPTED = ROS.cs_ros_message_ActionUpdate_get_INTERRUPTED();
	}

	//! An action that is performed instantly
	/*! i.e. has a starting time but no duration. */
	public class InstantAction : Action
	{
        DateTime time_to_start_action_;
	    InstantAction set_start_time(DateTime time_to_start_action)
	    {
            time_to_start_action_ = time_to_start_action;
            return this;
	    }
	}

	//! An action that has a known start and duration
	public class TransitionAction<T> : Action
	{
        DateTime time_to_start_action_;
        DateTime time_to_end_action_;

        public delegate void ActionStatusUpdate(T action_status);
        ActionStatusUpdate called_on_status_update_ = (T) => { };

        public TransitionAction<T> set_start_time(DateTime time_to_start_action)
        {
            time_to_start_action_ = time_to_start_action;
            return this;
        }

        public TransitionAction<T> set_end_time(DateTime time_to_end_action)
	    {
            time_to_end_action_ = time_to_end_action;
            return this;
	    }

	    public TransitionAction<T> set_status_callback(ActionStatusUpdate called_on_status_update)
	    {
            called_on_status_update_ = called_on_status_update;
            return this;
	    }

        public ActionStatusUpdate status_callback()
        {
            return called_on_status_update_;
        }
	}


	//! An action that has a known start, and an unknown duration
	public class UntimedAction<T>
	{
        DateTime time_to_start_action_;
        ActionStatusUpdate called_on_status_update_ = (T) => { };

	  public UntimedAction<T> set_start_time(DateTime time_to_start_action)
	  {
            time_to_start_action_ = time_to_start_action;
            return this;
	  }

	  public UntimedAction<T> set_status_callback(ActionStatusUpdate called_on_status_update)
	  {
			called_on_status_update_ = called_on_status_update;
            return this;
	  }

      public ActionStatusUpdate status_callback()
      {
          return called_on_status_update_;
      }

	  public delegate void ActionStatusUpdate(T action_status);


	}

	//! Logging interface
	public class Logging : ROSInterface
	{
		// Logging
		private ROS.Publisher ttsPub = null;
		private IntPtr ttsMessage = IntPtr.Zero;

		private ROS.Publisher ttsstopPub = null;
		IntPtr ttsstopMessage = IntPtr.Zero;

		private ROS.Publisher colorPub = null;
		IntPtr colorMessage = IntPtr.Zero;

		private ROS.Publisher touchPub = null;
        private IntPtr touchMessage = IntPtr.Zero;

		private ROS.Publisher buttonPressesPub = null;
		private IntPtr buttonPressMessage = IntPtr.Zero;


		void ROSInterface.setup_ROS_interfaces(ROS.NodeHandle nodeHandle, string topic_root)
		{
			touchPub = nodeHandle.advertise(topic_root + "/logging/tummy/touch", 0,
				ROS.cs_ros_message_Touch_publisher_ctor);
			touchMessage = ROS.cs_ros_message_Touch_create();

			ttsPub = nodeHandle.advertise(topic_root + "/logging/tts_start", 0,
				ROS.cs_ros_message_TTS_publisher_ctor);
			ttsMessage = ROS.cs_ros_message_TTS_create();

			ttsstopPub = nodeHandle.advertise(topic_root + "/logging/tts_stop", 0,
                ROS.cs_ros_message_TTSStop_publisher_ctor);
			ttsstopMessage = ROS.cs_ros_message_TTSStop_create();

			colorPub = nodeHandle.advertise(topic_root + "/logging/tummy/colours", 0,
                ROS.cs_ros_message_Colour_publisher_ctor);
			colorMessage = ROS.cs_ros_message_Colour_create();

            buttonPressesPub = nodeHandle.advertise(topic_root + "/logging/tummy/buttons", 0,
                ROS.cs_ros_message_ButtonPress_publisher_ctor);
            buttonPressMessage = ROS.cs_ros_message_ButtonPress_create();
		}

        public void log_touch(float x, float y, string tag)
		{
			IntPtr payload = ROS.cs_ros_message_Touch_get_pointer(touchMessage);
			ROS.cs_ros_message_Touch_set_x(payload, x);
			ROS.cs_ros_message_Touch_set_y(payload, y);

            IntPtr tag_ptr = ROS.cs_ros_message_Touch_get_pointer_tag(payload);
            ROS.cs_ros_string_set(tag_ptr, tag);

			IntPtr header = ROS.cs_ros_message_EyeMovement_get_pointer_header(payload);
			ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);

            IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
            ROS.cs_ros_time_set_now(time_stamp);

            touchPub.publish(touchMessage);

		}

        public void log_tts_start(string text)
		{
			IntPtr payload = ROS.cs_ros_message_TTS_get_pointer(ttsMessage);
			IntPtr text_ptr = ROS.cs_ros_message_TTS_get_pointer_text(payload);
            ROS.cs_ros_string_set(text_ptr, text);

			IntPtr header = ROS.cs_ros_message_TTS_get_pointer_header(payload);
			ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);

            IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
            ROS.cs_ros_time_set_now(time_stamp);

            ttsPub.publish(ttsMessage);
		}

        public void log_button_press(byte button, byte source, byte target)
        {
            IntPtr payload = ROS.cs_ros_message_ButtonPress_get_pointer(buttonPressMessage);
            ROS.cs_ros_message_ButtonPress_set_button(payload, button);
            ROS.cs_ros_message_ButtonPress_set_source(payload, source);
            ROS.cs_ros_message_ButtonPress_set_target(payload, target);

            IntPtr header = ROS.cs_ros_message_ButtonPress_get_pointer_header(payload);
            ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);

            IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
            ROS.cs_ros_time_set_now(time_stamp);


            buttonPressesPub.publish(buttonPressMessage);
        }

        public void log_colour(float r, float g, float b, bool left)
        {
            IntPtr payload = ROS.cs_ros_message_Colour_get_pointer(colorMessage);
            ROS.cs_ros_message_Colour_set_red(payload, r);
            ROS.cs_ros_message_Colour_set_green(payload, g);
            ROS.cs_ros_message_Colour_set_blue(payload, b);

            ROS.cs_ros_message_Colour_set_left(payload, (byte)(left?1:0));

            IntPtr header = ROS.cs_ros_message_Colour_get_pointer_header(payload);
            ROS.cs_ros_message_Header_set_seq(header, ROS.cs_ros_message_Header_get_seq(header) + 1);

            IntPtr time_stamp = ROS.cs_ros_message_Header_get_pointer_stamp(header);
            ROS.cs_ros_time_set_now(time_stamp);

            colorPub.publish(colorMessage);

        }

    }
}
