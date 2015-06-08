using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Behaviors.Channels;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Extensions to the SupervisorRef object.
    /// </summary>
    public static class SupervisorExtensions
    {
        /// <summary>
        /// Returns the agent ID of the child agent with specified child name.
        /// </summary>
        /// <param name="reference">The supervisor reference on which to retrieve the child.</param>
        /// <param name="childName">The internal name of the child to retrieve.</param>
        /// <returns>The agent ID of the child agent with specified child ID.</returns>
        public static string GetChildId(this SupervisorRef reference, string childName)
        { 
            Future<Response<SendChildId>> future =
                reference.SendFuture<Response<SendChildId>, SendChildId>(new SendChildId(childName, null));
            Response<SendChildId> response = future.Get;
            if (response == null)
                throw new SupervisorException("The call to retrieve the specified child ID failed to generate a response.");

            if (response.Body == null || string.IsNullOrEmpty(response.Body.Id))
                return string.Empty;
            return response.Body.Id;
        }

        /// <summary>
        /// Returns an array of child info objects containing 
        /// information on all children of this supervisor.
        /// </summary>
        /// <param name="reference">The supervisor reference on which to retrieve all children.</param>
        /// <returns>An array of child info objects containing 
        /// information on all children of this supervisor.</returns>
        public static ChildInfo[] GetAllChildren(this SupervisorRef reference)
        {
            Future<SendChildren> future = reference.SendFuture<SendChildren, GetChildren>(new GetChildren());
            SendChildren children = future.Get;
            if (children == null)
                throw new SupervisorException("The call to retrieve all supervisor children failed to generate a response.");

            return children.Children;
        }

        /// <summary>
        /// Returns a future as the result of the specified message to a supervisor.
        /// </summary>
        /// <typeparam name="TResult">The type of the future result.</typeparam>
        /// <typeparam name="TMessage">The type of the message to send.</typeparam>
        /// <param name="reference">The supervisor reference on which to retrieve a future value.</param>
        /// <param name="message">The message to send.</param>
        /// <returns></returns>
        public static Future<TResult> SendFuture<TResult, TMessage>(this SupervisorRef reference, TMessage message)
        {
            SupervisorRefChannel refChannel = reference as SupervisorRefChannel;
            if (refChannel != null)
            {
                AgentPort port = refChannel.GetPort();
                Future<TResult> fut = new FutureChannel<TResult>(port);
                fut.Send<TMessage>(message);
                return fut;
            }
            return null;
        }

        /// <summary>
        /// Dynamically adds a child specification to the supervisor 
        /// which starts the corresponding agent.
        /// </summary>
        /// <param name="reference">The supervisor reference on which to add the child specification.</param>
        /// <param name="spec">The specification for the child agent to start.</param>
        public static void StartChild(this SupervisorRef reference, ChildSpecification spec)
        {
            reference.Send<StartChild>(new StartChild(spec));
        }

        /// <summary>
        /// Commands the supervisor to restart the agent 
        /// corresponding to the specified child name.
        /// </summary>
        /// <param name="reference">The supervisor reference on which to restart the child agent.</param>
        /// <param name="childName">The internal name of the child to restart.</param>
        public static void RestartChild(this SupervisorRef reference, string childName)
        {
            reference.Send<RestartChild>(new RestartChild(childName));
        }

        /// <summary>
        /// Commands the supervisor to terminate the agent 
        /// corresponding to the specified child name.
        /// </summary>
        /// <param name="reference">The supervisor reference on which to stop the child agent.</param>
        /// <param name="childName">The internal name of the child to terminate.</param>
        public static void StopChild(this SupervisorRef reference, string childName)
        {
            reference.Send<StopChild>(new StopChild(childName));
        }

        /// <summary>
        /// Commands the supervisor to delete the child specification 
        /// corresponding to the specified child name.
        /// </summary>
        /// <param name="reference">The supervisor reference on which to delete the child specification.</param>
        /// <param name="childName">The internal name corresponding to the child specification to delete.</param>
        public static void DeleteChild(this SupervisorRef reference, string childName)
        {
            reference.Send<DeleteChild>(new DeleteChild(childName));
        }
    }
}
