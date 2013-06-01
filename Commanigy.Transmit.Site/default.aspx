<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="Commanigy.Transmit.Site.DefaultPage" Title="Transmit" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<link href="stylesheets/jquery.tooltip.css" rel="stylesheet" type="text/css" />
<script type="text/javascript" src="js/jquery.bgiframe.js"></script>
<script type="text/javascript" src="js/jquery.dimensions.js"></script>
<script type="text/javascript" src="js/jquery.tooltip.pack.js"></script>

<script type="text/javascript">
	$(function() {
		$("#share").hover(function() { $("#selected").addClass("share"); }, function() { $("#selected").removeClass("share"); });
		$("#request").hover(function() { $("#selected").addClass("request"); }, function() { $("#selected").removeClass("request"); });

		$('#selected a').tooltip({
			track: true,
			delay: 0,
			showURL: false,
			fade: 250,
			top: 20,
			left: -150
		});
	});
</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server">
		<% if (this.CurrentUser != null) { %>
			<div class="splitter">
				<div id="selected">
					<div class="grid_6 alpha">
						<a href="share.aspx" title="<%= _("<p>You can share one or more large files with others.</p> <p>If you are sharing files with colleagues, entering their name is sufficient, as their email address will be looked up automatically.</p> <p>Sharing files with others will require entering their full email address.</p>") %>"><img id="share" src="images/share-down.png" border="0" alt="Share files" /></a>
					</div>
					<div class="grid_6 omega">
						<a href="request.aspx" title="<%= _("<p>You can request others to share one or more files with you.</p> <p>If you are requesting files to be shared by colleagues, entering their name is sufficient, as their email address will be looked up automatically.</p><p>Requesting files to be shared by others will require entering their full email address.</p>") %>"><img id="request" src="images/request-down.png" border="0" alt="Request files" /></a>
					</div>
				</div>
			</div>
		<% } else { %>
			<h1><%= _("Overview") %></h1>
			<p>
				<%= _("This is the public interface of Transmit. This interface will be used if someone requests a file from you or if you need to download a file someone shared with you.") %>
			</p>
		<% } %>
	</form>
</asp:Content>