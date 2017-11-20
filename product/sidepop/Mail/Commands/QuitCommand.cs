using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents the Pop3 QUIT command.
	/// </summary>
	internal sealed class QuitCommand : Pop3Command<Pop3Response>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="QuitCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
        /// <param name="timeout">Timeout in minutes for the command to execute.</param>
        /// <param name="timeout">Timeout in minutes for the command to execute.</param>
        public QuitCommand(Stream stream, double timeout)
            : base(stream, false, Pop3State.Transaction | Pop3State.Authorization, timeout)
		{
		}

		/// <summary>
		/// Creates the Quit request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the QUIT request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Quit);
		}
	}
}