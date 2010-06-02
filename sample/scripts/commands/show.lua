Command("show","[<player>]","Shows a hidden player.",
  function(command,player,message)
    local target = player
    if message ~= "" then
      target = FindPlayer(message)
      if target == nil then
        Message("&eThere is no player '"..message.."'."):Send(player)
        return
      end
    end
    local name = nil
    if target ~= player then
      name = target.Name.." is"
    else name = "You are" end
    if target.Visible then
      Message("&e"..name.." is already visible."):Send(player)
      return
    end
    target.Visible = true
    Message("&e"..name.." now visible."):Send(player)
  end
)
