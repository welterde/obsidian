Command("lua","<code>","Executes a piece of LUA code.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
    local code = playerCode[player] or ""
    if message:sub(message:len()) == "\\" then
      message = message:sub(1,-2)
      playerCode[player] = code..message.."\n"
      Message("&e> "..message):Send(player)
      return
    end
    playerCode[player] = nil
    Message("&e> "..message):Send(player)
    local func,err = loadstring("do\n"..code..message.."\nend",player.Name)
    if func == nil then
      Message("&eError: "..err):Send(player)
    else
      local status,err = pcall(func)
      if not status then
        if type(err) == "string" then
          err = "&eError: "..err
        elseif type(err) == "userdata" then
          err = "&eError: "..err.Message
        else
          err = "Unknown errortype."
        end
        Message(err:split("\n",2)[0]):Send(player)
      end
    end
  end
)
