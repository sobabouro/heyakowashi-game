using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using Unity.VisualScripting;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Storage;
#endif



public class JoyconManager: MonoBehaviour
{

    // Settings accessible via Unity
    public bool EnableIMU = true;
    public bool EnableLocalize = true;

	// Different operating systems either do or don't like the trailing zero
	private const ushort vendor_id = 0x57e;
	private const ushort vendor_id_ = 0x057e;
	private const ushort product_l = 0x2006;
	private const ushort product_r = 0x2007;
    private const ushort usage_page = 0x0001;
    private const ushort usage_id = 0x0005;

    public List<Joycon> j; // Array of all connected Joy-Cons
    static JoyconManager instance;

    public static JoyconManager Instance
    {
        get { return instance; }
    }
    void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;

#if WINDOWS_UWP
		Debug.Log("WINDOWS_UWP");
		Task.Run(async () =>
		{				
			UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
				Debug.Log("Task start");
			}, true);
			try 
			{
				string selector = "System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:=1406 AND System.DeviceInterface.Hid.ProductId:=8198";
				UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
					Debug.Log($"selector: {selector}");
				}, true);

				DeviceInformationCollection collection = await DeviceInformation.FindAllAsync(selector);

				foreach (DeviceInformation inf in collection)
				{
					UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
						Debug.Log($"Id: {inf.Id}\nName: {inf.Name}\nKind: {inf.Kind}");
					}, true);

					HidDevice dev = await HidDevice.FromIdAsync(inf.Id, FileAccessMode.Read);

					if (dev != null)
					{
						UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
							Debug.Log($"Succeeded to open HID");
						}, true);
					}
					else
					{
						UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
							Debug.Log($"Failed to open HID");
						}, true);

						var dai = DeviceAccessInformation.CreateFromId(inf.Id);

						UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
							Debug.Log($"CurrentStatus:{dai.CurrentStatus.ToString()}");
						}, true);
					}
				}
			}
			catch (Exception e)
			{
				UnityEngine.WSA.Application.InvokeOnAppThread(()=>{
					Debug.Log($"Exception: {e.ToString()}");
				}, true);
			}
		});
#endif

        int i = 0;

		j = new List<Joycon>();
		bool isLeft = false;
		HIDapi.hid_init();

		IntPtr ptr = HIDapi.hid_enumerate(vendor_id, 0x0);
		IntPtr top_ptr = ptr;

		if (ptr == IntPtr.Zero)
		{
			ptr = HIDapi.hid_enumerate(vendor_id_, 0x0);
			if (ptr == IntPtr.Zero)
			{ 
				HIDapi.hid_free_enumeration(ptr);
				Debug.Log ("No Joy-Cons found!");
			}
		}
		hid_device_info enumerate;
		while (ptr != IntPtr.Zero) {
			enumerate = (hid_device_info)Marshal.PtrToStructure (ptr, typeof(hid_device_info));

            Debug.Log($"vendor_id: {enumerate.vendor_id}");
            Debug.Log($"product_id: {enumerate.product_id}");
            Debug.Log($"usage_page: {enumerate.usage_page}");
            Debug.Log($"usage: {enumerate.usage}");

            if (enumerate.product_id == product_l || enumerate.product_id == product_r) {
					if (enumerate.product_id == product_l) {
						isLeft = true;
						Debug.Log ("Left Joy-Con connected.");
					} else if (enumerate.product_id == product_r) {
						isLeft = false;
						Debug.Log ("Right Joy-Con connected.");
					} else {
						Debug.Log ("Non Joy-Con input device skipped.");
					}
					IntPtr handle = HIDapi.hid_open_path (enumerate.path);
					HIDapi.hid_set_nonblocking (handle, 1);
					j.Add (new Joycon (handle, EnableIMU, EnableLocalize & EnableIMU, 0.05f, isLeft));
					++i;
				}
				ptr = enumerate.next;
			}
		HIDapi.hid_free_enumeration (top_ptr);
    }

    void Start()
    {
		for (int i = 0; i < j.Count; ++i)
		{
			Debug.Log (i);
			Joycon jc = j [i];
			byte LEDs = 0x0;
			LEDs |= (byte)(0x1 << i);
			jc.Attach (leds_: LEDs);
			jc.Begin ();
		}
    }

    void Update()
    {
		for (int i = 0; i < j.Count; ++i)
		{
			j[i].Update();
		}
    }

    void OnApplicationQuit()
    {
		for (int i = 0; i < j.Count; ++i)
		{
			j[i].Detach ();
		}
    }
}
