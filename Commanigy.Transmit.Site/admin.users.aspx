<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="admin.users.aspx.cs"
	Inherits="Commanigy.Transmit.Site.AdminUsersPage" Title="Users" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server" class="admin users">
		<h1><a href="admin.settings.aspx">Settings</a> <b>Users</b></h1>
			<asp:ValidationSummary ID="ValidationSummary" runat="server" EnableViewState="False" />

		<p>
			Create a new local user. User will be added to "Transmit Users" group and may be used
			for demonstration purposes.
		</p>
		<fieldset class="authentication">
			<legend>Authentication</legend>
			<fieldset>
				<asp:Label AssociatedControlID="TbxName" runat="server">Login: <span class="req">*</span></asp:Label><br />
				<asp:TextBox ID="TbxName" runat="server" />
				<p class="hint">Windows Authentication login</p>
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxPassword" runat="server">Password: <span class="req">*</span></asp:Label><br />
				<asp:TextBox ID="TbxPassword" runat="server" TextMode="Password" />
			</fieldset>
			<fieldset>
				<p class="hint">Both of these should be distributed to users testing Transmit.</p>
			</fieldset>
			<div class="clear"></div>
		</fieldset>
		<fieldset class="user">
			<legend>User</legend>
			<fieldset>
				<asp:Label AssociatedControlID="TbxFullName" runat="server">Full name:</asp:Label><br />
				<asp:TextBox ID="TbxFullName" runat="server" />
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxMail" runat="server">Mail:</asp:Label><br />
				<asp:TextBox ID="TbxMail" runat="server" />
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxUrl" runat="server">Url:</asp:Label><br />
				<asp:TextBox ID="TbxUrl" runat="server" />
				<p class="hint">Full URL to image of user</p>
			</fieldset>
		</fieldset>
		<fieldset class="company">
			<legend>Company</legend>
			<fieldset>
				<asp:Label AssociatedControlID="TbxCompany" runat="server">Company:</asp:Label><br />
				<asp:TextBox ID="TbxCompany" runat="server" />
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxTitle" runat="server">Title:</asp:Label><br />
				<asp:TextBox ID="TbxTitle" runat="server" />
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxDepartment" runat="server">Department:</asp:Label><br />
				<asp:TextBox ID="TbxDepartment" runat="server" />
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxCountry" runat="server">Country:</asp:Label><br />
				<asp:TextBox ID="TbxCountry" runat="server" />
			</fieldset>
			<fieldset>
				<asp:Label AssociatedControlID="TbxLocation" runat="server">Location:</asp:Label><br />
				<asp:TextBox ID="TbxLocation" runat="server" />
			</fieldset>
		</fieldset>

		<asp:Button ID="BtnCreateUser" runat="server" Text="Create User" 
			onclick="BtnCreateUser_Click" />

		<h3>Existing local users</h3>
		<div class="existing-users">
			<asp:ListView ID="DlLocalUsers" runat="server">
				<LayoutTemplate>
					<table id="logs-overview">
						<tr>
							<th>Name</th>
							<th>Full name</th>
						</tr>
						<tr runat="server" id="itemPlaceholder"></tr>
					</table>
				</LayoutTemplate>
				<ItemTemplate>
					<tr>
						<td><%# Eval("Name") as string %></td>
						<td><%# Eval("FullName") as string%> <span class="description"><%# Eval("Description") as string %></span></td>
					</tr>
				</ItemTemplate>
			</asp:ListView>
		</div>
	</form>
</asp:Content>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		$(document).ready(function() {
		
		});
	</script>

</asp:Content>
