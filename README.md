## Project Earth Api
*The core API for Project Earth*
Fork of poiuytrezaur

## DISCLAIMER
The API implementation is NOT complete, which means that not all of the game features might work as expected.
### Special thanks to [LukeFZ](https://github.com/LukeFZ) for providing all of the necessary data for recovering missing features

## What does this component do?
The core API handles the bulk of game functionality - pretty much everything that isn't direct AR gameplay is done here.

| Currently working features               | Partially working features                            | 
|------------------------------------------|-------------------------------------------------------|
| Map                                      | Buildplates (Implemetation is not complete)           |
| Tappables                                | Adventures/Encounters (Implemetation is not complete) |
| Crafting                                 | Smelting (Implementation is not complete)             |
| Store                                    | Buildplate sharing (Webpage is not implemented)       |
| Inventory                                | Challenges (Challenge conditions are broken)          |
| Boosts (Do not confuse with boost minis) |                                                       |
| Journal                                  |                                                       |
| Activity Log                             |                                                       |

## Building
1. `git clone --recursive https://github.com/jackcaver/Api.git`
2. `cd Api`
3. `dotnet build` or use any IDE that you want and build it there

## Setting up the Project Earth server infrastructure.

### Getting all the parts

to start, ensure that you have built copies of all the required components downloaded:
- A built copy of the Api (you are in this repo), which you can get from [GitHub Actions](https://github.com/jackcaver/Api/actions/workflows/build.yml)
- My [ApiData](https://github.com/jackcaver/ApiData) repo, or your own data. Rename your clone to `data`, and place it next to your Api executable. (if you are using a GitHub Actions build or if you got the API by using `git clone --recursive`, you can skip this step)
- In addition, you'll need the Minecraft Earth resource pack file, renamed to `vanilla.zip` and placed in the `data/resourcepacks`. You can procure the resourcepack from [here](https://cdn.mceserv.net/availableresourcepack/resourcepacks/dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35), provided you're setting up before June 30th, 2021.
- Our fork of [Cloudburst](https://github.com/Project-Earth-Team/Server). Builds of this can be found [here](https://ci.rtm516.co.uk/job/ProjectEarth/job/Server/job/earth-inventory/). This jar can be located elsewhere from the Api things.
- Run Cloudburst once to generate the file structure.
- In the plugins folder, you'll need [GenoaPlugin](https://github.com/jackcaver/GenoaPlugin), and [GenoaAllocatorPlugin](https://github.com/jackcaver/GenoaAllocatorPlugin). The CI for this can be found [here](https://github.com/jackcaver/GenoaPlugin/actions/workflows/CI.yml) and [here](https://github.com/jackcaver/GenoaAllocatorPlugin/actions/workflows/CI.yml). **Note: make sure to rename your GenoaAllocatorPlugin.jar to ZGenoaAllocatorPlugin.jar, or you will run into issues with class loading** 

### Setting up

On the cloudburst side:
- within the `plugins` folder, create a `GenoaAllocatorPlugin` folder, and in there, make a `key.txt` file containing a base64 encryption key and `ip.txt` file containing your server's ip address. An example key is
 ```
/g1xCS33QYGC+F2s016WXaQWT8ICnzJvdqcVltNtWljrkCyjd5Ut4tvy2d/IgNga0uniZxv/t0hELdZmvx+cdA==
```
- edit the cloudburst.yml file, and chan ge the core api url to the url your Api will be accessible from
- on the Api side, go to `data/config/apiconfig.json`, and add the following:
```json
"multiplayerAuthKeys": {
        "The same IP you put in ip.txt": "the same key you put in key.txt earlier"
 }
```
- Start up the Api
- Start up cloudburst. After a short while the Api should mention a server being connected.
- If you run into issues, retrace your steps, or [contact us on discord](https://discord.gg/Zf9aYZACU4)
- If everything works, your next challenge is to get Minecraft Earth to talk to your Api. If you're on Android, you can utilize [our patcher](https://github.com/Project-Earth-Team/PatcherApp). If you're on IOS, the only way to accomplish this without jailbreak is to utilize a DNS, such as bind9. Setup for that goes beyond the scope of this guide.


