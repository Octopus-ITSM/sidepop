using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This command represents the Pop3 RSET command.
	/// This resets (unmarks) any messages previously marked for deletion in this session so that the QUIT command will not delete them.
	/// </summary>
	internal sealed class ResetCommand : Pop3Command<Pop3Response>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResetCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
        /// <param name="timeout">Timeout in minutes for the command to execute.</param>
        public ResetCommand(Stream stream, double timeout)
            : base(stream, false, Pop3State.Transaction, timeout)
		{
		}

		/// <summary>
		/// Creates the RSET request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the RSET request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Rset);
		}
	}
}