<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="help.aspx.cs" Inherits="Commanigy.Transmit.Site.HelpPage" Title="Help" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<div class="grid_8 alpha">
		<h2><%= _("Frequently asked questions") %></h2>
		<ul id="faq">
			<li><a href="#">How do I share a file?</a>
				<div class="faq-item">
					<b>Sharing with colleagues</b><br />
					<p>If you are sharing files with colleagues, entering their name is sufficient, as their email address will be looked up automatically.</p>

					<b>Sharing with others</b><br />
					<p>Sharing files with others will require entering their full email address.</p>

					<p>The recipients will automatically receive an email notification with your message enabling them to access the files.</p>
				</div>
			</li>
			<li><a href="#">How do I request people to send me a file?</a>
				<div class="faq-item">
					<p>You can request others to share one or more files with you.</p>

					<b>Request colleagues</b><br />
					<p>If you are requesting files to be shared by colleagues, entering their name is sufficient, as their email address will be looked up automatically.</p>

					<b>Request others</b><br />
					<p>Requesting files to be shared by others will require entering their full email address.</p>

					<p>Once you have sent off your request for file sharing, the recipient will receive an email notification with your request and a unique ID enabling him/her to upload the requested documents.</p>

					<p>When the files have been uploaded, you will receive an email notification.</p>
				</div>
			</li>
		</ul>

	</div>
	
	<div class="grid_4 omega">
		<div class="sidebar info">
			<h2><%= _("Help") %></h2>
			<p>
				<%= _("This section helps you get started with Transmit.") %>
			</p>
		</div>
	</div>	
</asp:Content>