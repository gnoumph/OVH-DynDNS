# OVH-DynDNS

This program allow you to automatically update your DNS zone when your public IP is renewed.  
It works with OVH and can send you a SMS when it's done (works only with Free Mobile).

## Updates notes

This program now supports Telegram notifications and is build with Visual Studio 2019 Community Edition.  
Two new parameters are availables (and facultatives):

```
{
  …
  "TelegramAccessToken":"{telegramAccessToken}",
  "TelegramChatId":"{telegramChatId}",
  …
}
```

As I have another ISP, with a public static IP packed with my subscription, I don't thing I'll add anything here in future.

## What's your needs?

### Visual Studio 2017 Community Edition

Download and install Visual Studio 2017 Community Edition to be able to open and compile the solution. The Community Edition is free and meets your needs.  
https://visualstudio.microsoft.com/

### OVH token

First, you need an API token from OVH. It allows you to manage your account from any custom app.

Go to https://api.ovh.com/createToken/ and set your account informations, set **Validity** to _Unlimited_ then set **Rights** like this:

 * **PUT** /domain/zone/{zoneName}/dynHost/record/*
 * **POST** /domain/zone/{zoneName}/refresh
 * **GET** /domain/zone/{zoneName}/dynHost/record
 * **GET** /domain/zone/{zoneName}/dynHost/record/*

Of course, replace **{zoneName}** with... your zone name.  
Create your keys, note them.

### Free Mobile key (optionnal)

This part is fully optionnal, and is needed only if you want to receive a notification by SMS when your DNS zone is updated. It supports **only** Free Mobile.

Go to https://mobile.free.fr/moncompte/, then click on **Gérer mon compte** and **Mes options**. Scroll down then activate the **Notifications par SMS** option.  
Note your credentials.

### Config file

The config file is a simple JSON file.

Create an **OVH DynDNS** folder in **%AppData%** (usually ```C:\Users\{username}\AppData\Roaming```).  
In this folder, create a **config.json** file, then write this JSON object:

```
{
  "OvhRegion":"ovh-eu",
  "ApplicationKey":"{ovhApplicationKey}",
  "ApplicationSecret":"{ovhApplicationSecret}",
  "ConsumerKey":"{ovhConsumerKey}",
  "ZoneName":"{ovhZoneName}",
  "SmsUser":"{freeMobileSmsUser}",
  "SmsPass":"{freeMobileSmsPass}"
}
```

As said before, the **SmsUser** and **SmsPass** are fully optionnal and can be ignored.

### Set a custom script to get your current public IP (optionnal)

If you look at the source code, you'll notice at line 31 a call to https://api.ipify.org/.  
It's needed to get the actual public IP. But if you don't want to use an external ressource that you can't control, just add this small PHP script on your web server then replace the address by your own.

```
<?php
echo $_SERVER['REMOTE_ADDR'];
?>
```

That's it!

## Compiling the application

Now you are ready to compile the app.

Open the **OVH DynDNS.sln** file. If it's not the case, select **Release** in the dropdown list (**Debug** should be selected by default) in the top toolbar.  
Now you can compile the program by pressing **F6**.

A new folder is created in your project directory. Navigate to ```...\OVH DynDNS\bin\Release``` then copy everything where you want to store the program.

Note the path.

## Set a scheduled task

Now you have to create a new scheduled task to start the program automatically periodically.

Open the tasks manager then create a new task.

In **General** tab, set the task **Name**, check **Run whether user is logged on or not** and **Run with highest privileges**, and in **Configure for** select your Windows version.

In **Triggers** tab, click on **New...**. Begin the task **At startup**, repeat task every **1 minutes** (or everything else you want) for a duration of **Indefinitely**.

In **Actions** tab, click on **New...** then on **Browse...**. Navigate to your application path, then select **OVH DynDNS.exe**.

In **Conditions** tab, uncheck everything.

In **Settings** tab, check **Run task as soon as possible after a scheduled start is missed** and uncheck **Stop the task if it runs longer than**.

Validate, then you're ready!

## How to improve? What's next?

 * Add more registars support
 * Add more SMS providers support
 * Create the config file from the program
 * Automatically create the scheduled task (or use another way to check periodically)
 * Create an installer for non-developper users
 * Find a better way to get current public IP (and don't use an external URL)
 * Create an UI
 * Probably a lot of things!

## Why this program?

Because my ISP want my money every months for a static public IP, and my server is hosted on my personnal computer.  
So I wrote this small program, and I want to share it with everyone, even if I don't think I'll continue to work on it for now.
