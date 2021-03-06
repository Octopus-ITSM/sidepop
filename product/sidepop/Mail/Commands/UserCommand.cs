using System;
using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This command represents a Pop3 USER command.
	/// </summary>
	internal sealed class UserCommand : Pop3Command<Pop3Response>
	{
		private readonly string _username;

		/// <summary>
		/// Initializes a new instance of the <see cref="UserCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="username">The username.</param>
        /// <param name="timeout">Timeout in minutes for the command to execute.</param>
        public UserCommand(Stream stream, string username, double timeout)
            : base(stream, false, Pop3State.Authorization, timeout)
		{
			if (string.IsNullOrEmpty(username))
			{
				throw new ArgumentNullException("username");
			}

			_username = username;
		}

		/// <summary>
		/// Creates the USER request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the USER request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.User, _username, Pop3Commands.Crlf);
		}
	}
}