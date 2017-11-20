using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Keepass2android.Pluginsdk;
using KeePassLib;

namespace keepass2android.AutoFillPlugin
{
	[Activity(Label = "@string/LookupTitle", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance, Theme="@style/android:Theme.Material.Light")]
    public class LookupCredentialsActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			var url = Intent.GetStringExtra("UrlToSearch");
            
            StartActivityForResult(Kp2aControl.GetQueryEntryIntent(url), 123);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

			try
			{

				string lastQueriedUrl = data.GetStringExtra(Strings.ExtraQueryString);


				string entryId = data.GetStringExtra(Strings.ExtraEntryId);
	            var jsonOutput = new Org.Json.JSONObject(data.GetStringExtra(Strings.ExtraEntryOutputData));
	            Dictionary<string, string> output = new Dictionary<string, string>();
	            for (var iter = jsonOutput.Keys(); iter.HasNext;)
	            {
	                string key = iter.Next().ToString();
	                string value = jsonOutput.Get(key).ToString();
	                output[key] = value;
	            }


	            string user = "", password = "";
	            output.TryGetValue(KeePassLib.PwDefs.UserNameField, out user);
	            output.TryGetValue(KeePassLib.PwDefs.PasswordField, out password);
				Android.Util.Log.Debug ("KP2AAS", "Received credentials for " + lastQueriedUrl);
				bool hasData = false;
				Keepass2android.Kbbridge.KeyboardDataBuilder kbdataBuilder = new Keepass2android.Kbbridge.KeyboardDataBuilder();

				String[] keys = {PwDefs.UserNameField, 
							PwDefs.PasswordField, 
				
						};
				
				int i = 0;
				foreach (string key in keys)
				{
					String value;
					
					if (output.TryGetValue(key, out value) && (value.Length > 0))
					{
						kbdataBuilder.AddString(key, keys[i], value);
						hasData = true;
					}
					i++;
				}
				
				kbdataBuilder.Commit();
				string title;
				output.TryGetValue(PwDefs.TitleField, out title);
				if (string.IsNullOrEmpty(title))
					title = "untitled";
				Keepass2android.Kbbridge.KeyboardData.EntryName = title;
				Keepass2android.Kbbridge.KeyboardData.EntryId = entryId;
				if (hasData)
					Keepass2android.Autofill.AutoFillService.NotifyNewData(lastQueriedUrl);

			}
			catch(Exception e) {
				Android.Util.Log.Debug ("KP2AAS", "Exception while receiving credentials: " + e.ToString());
			}
			finally {
				
				Finish ();
			}
        }

        
    }
}