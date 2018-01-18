﻿//
//  Program.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.IO;

using Launchpad.Launcher.Handlers;
using Launchpad.Launcher.Interface;
using log4net;

namespace Launchpad.Launcher
{
	/// <summary>
	/// The main program class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The config handler reference.
		/// </summary>
		private static readonly ConfigHandler Config = ConfigHandler.Instance;

		/// <summary>
		/// Logger instance for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			// Bind any unhandled exceptions in the main thread so that they are logged.
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			log4net.Config.XmlConfigurator.Configure();

			Log.Info("----------------");
			Log.Info($"Launchpad v{Config.GetLocalLauncherVersion()} starting...");
			Log.Info($"Current platform: {ConfigHandler.GetCurrentPlatform()} ({(Environment.Is64BitOperatingSystem ? "x64" : "x86")})");

			// Set correct working directory for compatibility with double-clicking
			Directory.SetCurrentDirectory(ConfigHandler.GetLocalDir());

			Log.Info("Initializing UI...");

			// Bind any unhandled exceptions in the GTK UI so that they are logged.
			GLib.ExceptionManager.UnhandledException += OnGLibUnhandledException;

			// Run the GTK UI
			Gtk.Application.Init();
			var win = new MainWindow();
			win.Show();
			Gtk.Application.Run();
		}

		/// <summary>
		/// Passes any unhandled exceptions from the GTK UI to the generic handler.
		/// </summary>
		/// <param name="args">The event object containing the information about the exception.</param>
		private static void OnGLibUnhandledException(GLib.UnhandledExceptionArgs args)
		{
			OnUnhandledException(null, args);
		}

		/// <summary>
		/// Event handler for all unhandled exceptions that may be encountered during runtime. While there should never
		/// be any unhandled exceptions in an ideal program, unexpected issues can and will arise. This handler logs
		/// the exception and all relevant information to a logfile and prints it to the console for debugging purposes.
		/// </summary>
		/// <param name="sender">The sending object.</param>
		/// <param name="unhandledExceptionEventArgs">The event object containing the information about the exception.</param>
		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			Log.Fatal("----------------");
			Log.Fatal("FATAL UNHANDLED EXCEPTION!");
			Log.Fatal("Something has gone terribly, terribly wrong during runtime.");
			Log.Fatal("The following is what information could be gathered by the program before crashing.");
			Log.Fatal
			(
				"Please report this to <jarl.gullberg@gmail.com> or via GitHub. Include the full log and a " +
				"description of what you were doing when it happened."
			);

			if (!(unhandledExceptionEventArgs.ExceptionObject is Exception unhandledException))
			{
				return;
			}

			if (unhandledException is DllNotFoundException)
			{
				Log.Fatal
				(
					"This exception is typical of instances where the GTK# runtime has not been installed.\n" +
					"If you haven't installed it, download it at \'http://www.mono-project.com/download/#download-win\'.\n" +
					"If you have installed it, reboot your computer and try again."
				);

				// Send the user to the common problems page.
				System.Diagnostics.Process.Start("https://github.com/Nihlus/Launchpad/wiki/Common-problems");
			}

			Log.Fatal("Exception type: " + unhandledException.GetType().FullName);
			Log.Fatal("Exception Message: " + unhandledException.Message);
			Log.Fatal("Exception Stacktrace: " + unhandledException.StackTrace);
		}
	}
}
