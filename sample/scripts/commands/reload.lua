Command("reload","<command>","Reloads a command.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
    local func,err = loadfile("scripts/commands/"..message..".lua",player.Name)
    if func == nil then
      Message("&eError: "..err):Send(player)
    else
      local status,err = pcall(func)
      if status then
        Message("&eReloaded command '"..message.."'."):Send(player)
      else
        if type(err) == "string" then
          err = "&eError: "..err
        elseif type(err) == "userdata" then
          err = "&eError: "..err.Message
        end
        Message(err:split("\n",2)[0]):Send(player)
      end
    end
  end
)