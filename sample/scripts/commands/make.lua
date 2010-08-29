Command("make","<account> <group>","Changes the group of a player.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local args = message:split(" ",3)
    if #args == 3 then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local target = FindPlayer(args[1])
    if target == nil then
      target = FindAccount(args[1])
      if target == nil then
        Message("&eUnknown player or account '"..args[1].."'."):Send(player)
        return
      end
    else
      target = target.Account
    end
    local group = FindGroup(args[2])
    if group == nil then
      Message("&eUnknown group '"..args[2].."'."):Send(player)
      return
    end
    if target.Group == group then
      Message("&e"..target.Name.." already is in this group."):Send(player)
      return
    end
    local node = player.Group.Custom["changegroup"]
    if node and not node:Contains(target.Group.Name) then
      Message("&eYou can't change the group of an "..target.Group.Name.."."):Send(player)
      return
    end
    Message("&e*"..target.Group.Prefix..target.Name..target.Group.Postfix.."&e is now "..
            group.Prefix..group.Name..group.Postfix.."&e."):Send(player.Level)
    target.Group = group
    target:Save()
  end
)
