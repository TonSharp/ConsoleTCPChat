# Simple Console TCP Chat
**Status:**    

![GitHub Workflow Status](https://img.shields.io/github/workflow/status/TonSharp/Console-TCPChat/.NET?logo=Build)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/TonSharp/Console-TCPChat?logo=Size)
![GitHub](https://img.shields.io/github/license/TonSharp/Console-TCPChat?logo=License)
![GitHub last commit](https://img.shields.io/github/last-commit/TonSharp/Console-TCPChat)    
![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/TonSharp/Console-TCPChat?include_prereleases)

![GitHub followers](https://img.shields.io/github/followers/TonSharp?label=Follow&style=social)
![GitHub watchers](https://img.shields.io/github/watchers/TonSharp/Console-TCPChat?label=Watch&style=social)

:computer: :mailbox_with_mail: 

Content:
 - [What is the TCPChat?](#What-is-the-TCPChat)
 - [Why do I need TCPChat?](#Why-do-I-need-TCPChat)
 - [What are the advantages of TCPChat?](#What-are-the-advantages-of-TCPChat)
 - [Development plan](#Development-plan)
 - [Technical Specification](#Technical-Specification)
	 - [Client Side](#Client-Side)
	 - [Server Side](#Server-Side)
	 - [Message system](#Message-system)
	 - [Commands](#Commands)
	 - [Console Options](#Console-Options)

# What is the TCPChat?

TCPChat is a simple .NET Core application that allows you to communicate with other users, via the console and the **TCP** protocol. You can create a room with **any port**, or connect to an existing one.

# Why do I need TCPChat?

TCP Chat allows you to communicate without any registration or use of personal data of users. Just run and use. We do not use any third-party intermediate servers that store correspondence, all clients connect directly to the server on a remote computer. You can also use console options to speed up the connection process.

# What are the advantages of TCPChat?

I do not think that this project has any advantages. It was created for personal purposes. But still, in this project, you can highlight some features. It is designed in a retro style, which will allow you to feel the nostalgic atmosphere. You can use the console options for the shortcut to quickly set up communication with the remote computer. Support for sounds and RGB also improve the experience of interacting with the app.

TCP Chat supports **RGB** color **nicknames**, **commands** for interacting with the server and clients (in development), console **options**. Many other cool features will appear in the future)

TCP Chat is a completely anonymous application, all you need is to enter any nickname and color.

# Development plan

:white_check_mark: Basic ability to send messages;    
:white_check_mark: RGB nickname support;    
:white_check_mark: Console options support;    
:white_check_mark: Sound support;    
:white_large_square: Administrator system (commands, e.t.c.);    
:white_large_square: Encryption support;    
:white_large_square: File transfer support;    

# Technical Specification

## Client Side

Clients connect to the server and communicate with each other only through the server. They don't know anything about each other, which allows for greater anonymity. The server gives each client its unique **ID** address once when connecting, which is used later for identification (in the future for decrypting messages). 

Attackers will not be able to decrypt these messages, as they will not have a unique address.

## Server Side

The server has information about each client, and when connected, it receives the client's userdata. Other clients receive public **UserData** from the server only when this client sends a message, it consists of a **UserName** and a **Color**. At any other time, the server does not send data to other clients, which provides increased security and anonymity.

## Message system

All requests from clients and the server are sent in the form of so-called **Messages**. The first four bytes consist of an integer called a **PostCode**, it shows the content of the message.

Available **PostCodes**:

|PostCodes|Description|
|--|--|
|1-4|Usual messages from client or server|
|5| Request from client for **ID**|
|6|Request from client for **UserData**|
|7|Reserved|
|8|Client connecting message|
|9|Client disconnecting message|
|10|Server disconnecting message|
|11|Sending client **ID** from Server|
|12|Sending client **UserData** from Server|

In the full version of Message, the **PostCode** is followed by the public **UserData**, followed by the message **Text**. Depending on the **PostCode**, the Message may not contain **UserData** or the message **Text**.

## Commands

Here is a list of currently available commands:

### General commands

|Command|Description|
|--|--|
|**/join** [adress]:[port]|Connect to the server|
|/join [adress] [port]||
|**/connect** [adress]:[port]|The same as /join|
|/connect [adress] [port]||
|**/room** [port]|Start server|
|**/create** [port]|The same as /room|
|**/disconnect**|Disconnect from server or stop server|
|**/dconnect**|The same as /disconnect|
|**/clear**|Clears screen|
|**/clr**|The same as /clear|

### User commands

|Command|Description|
|--|--|
|/color [color]|Sets up user color|


## Console Options

| Option | Description |
|--|--|
|**-N** [name]|Sets up **UserName** (-c required)|
|--name [name]|The same as -N|
|**-c** [color]|Sets up user **Color** (-N required)|
|--color|The same as -c|
|**-S** [port]|If -N and -c seted up, starts server|
|--server [port]|The same as -S|
|**-C** [hostname] [port] or [hostname:port]|If -N and -c seted up, connects to the server|
|--client [hostname] [port] or [hostname:port]|The same as -C|
