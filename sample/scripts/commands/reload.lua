Command("reload","<command>{,<command>}","Reload commands.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local commands = message:split(",")
    for i = 1,#commands do
      local command = commands[i]
      local func,err = loadfile("scripts/commands/"..command..".lua",player.Name)
      if func == nil then
        Message("&eError: "..err):Send(player)
      else
        local status,err = pcall(func)
        if status then
          Message("&eReloaded command '"..command.."'."):Send(player)
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
  end
)
