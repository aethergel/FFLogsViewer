using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

namespace FFLogsViewer.API;

public class ChatTwoIpc(IDalamudPluginInterface pluginInterface)
{
    private ICallGateSubscriber<string> Register { get; } = pluginInterface.GetIpcSubscriber<string>("ChatTwo.Register");
    private ICallGateSubscriber<string, object?> Unregister { get; } = pluginInterface.GetIpcSubscriber<string, object?>("ChatTwo.Unregister");
    private ICallGateSubscriber<object?> Available { get; } = pluginInterface.GetIpcSubscriber<object?>("ChatTwo.Available");
    private ICallGateSubscriber<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?> Invoke { get; } = pluginInterface.GetIpcSubscriber<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?>("ChatTwo.Invoke");

    private string? registrationId;

    public void Enable()
    {
        this.Available.Subscribe(this.RegisterIpc);
        this.RegisterIpc();

        this.Invoke.Subscribe(this.Integration);
    }

    public void Disable()
    {
        if (this.registrationId != null)
        {
            this.Unregister.InvokeAction(this.registrationId);
            this.registrationId = null;
        }

        this.Invoke.Unsubscribe(this.Integration);
    }

    private void RegisterIpc()
    {
        try
        {
            this.registrationId = this.Register.InvokeFunc();
        }
        catch (IpcNotReadyError)
        {
            // ignored
        }
    }

    private void Integration(string id, PlayerPayload? sender, ulong contentId, Payload? payload, SeString? senderString, SeString? content)
    {
        if (id != this.registrationId)
        {
            return;
        }

        if (ImGui.Selectable($"[F] {Service.Configuration.ContextMenuButtonName}"))
        {
            if (payload is not PlayerPayload playerPayload)
            {
                return;
            }

            if (playerPayload.PlayerName != string.Empty)
            {
                if (playerPayload.World.RowId != 0 && !Util.IsWorldValid(playerPayload.World.Value))
                {
                    return;
                }

                var playerName = playerPayload.World.RowId != 0
                                     ? $"{playerPayload.PlayerName}@{playerPayload.World.Value.Name}"
                                     : playerPayload.PlayerName; // this is only fine due to being 100% a player payload

                ContextMenu.SearchPlayer(playerName);
            }
        }
    }
}
