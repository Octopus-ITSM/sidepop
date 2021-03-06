using System;
using System.Collections.Generic;
using System.IO;
using sidepop.Mail.Responses;
using sidepop.Mail.Results;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents both the multiline and single line Pop3 LIST command.
	/// </summary>
	internal sealed class ListCommand : Pop3Command<ListResponse>
	{
		// the id of the message on the server to retrieve.
		private readonly int _messageId;

		public ListCommand(Stream stream, double timeout)
            : base(stream, true, Pop3State.Transaction, timeout)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="messageId">The message id.</param>
        /// <param name="timeout">Timeout in minutes for the command to execute.</param>
        public ListCommand(Stream stream, int messageId, double timeout)
			: this(stream, timeout)
		{
			if (messageId < 0)
			{
				throw new ArgumentOutOfRangeException("messageId");
			}

			_messageId = messageId;

			base.IsMultiline = false;
		}

		/// <summary>
		/// Creates the LIST request message.
		/// </summary>
		/// <returns>The byte[] containing the LIST request message.</returns>
		protected override byte[] CreateRequestMessage()
		{
			string requestMessage = Pop3Commands.List;

			if (!IsMultiline)
			{
				requestMessage += " " + _messageId.ToString();
			} // Append the message id to perform the LIST command for.

			return GetRequestMessage(requestMessage, Pop3Commands.Crlf);
		}

		/// <summary>
		/// Creates the response.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <returns>A <c>ListResponse</c> containing the results of the Pop3 LIST command.</returns>
		protected override ListResponse CreateResponse(byte[] buffer)
		{
			Pop3Response response = Pop3Response.CreateResponse(buffer);

			List<Pop3ListItemResult> items;

			if (IsMultiline)
			{
				items = new List<Pop3ListItemResult>();
				string[] values;
				string[] lines = GetResponseLines(StripPop3HostMessage(buffer, response.HostMessage));

				foreach (string line in lines)
				{
					//each line should consist of 'n m' where n is the message number and m is the number of octets
					values = line.Split(' ');
					if (values.Length < 2)
					{
						throw new Pop3Exception(string.Concat("Invalid line in multiline response:  ", line));
					}

					items.Add(new Pop3ListItemResult(Convert.ToInt32(values[0]),
					                                 Convert.ToInt64(values[1])));
				}
			} //Parse the multiline response.
			else
			{
				items = new List<Pop3ListItemResult>(1);
				string[] values = response.HostMessage.Split(' ');

				//should consist of '+OK messageNumber octets'
				if (values.Length < 3)
				{
					throw new Pop3Exception(string.Concat("Invalid response message: ", response.HostMessage));
				}
				items.Add(new Pop3ListItemResult(Convert.ToInt32(values[1]), Convert.ToInt64(values[2])));
			} //Parse the single line results.

			return new ListResponse(response, items);
		}
	}
}