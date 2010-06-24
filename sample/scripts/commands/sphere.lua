Command("sphere","[hollow] <radius> [<block>]","Draws a sphere.",
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
    local radius = tonumber(args[1])
    if radius == nil then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local b = nil
    if #args == 2 then
      local n = tonumber(args[2])
      if n then b = Blocktype.FindById(n)
      else b = Blocktype.FindByName(args[2]) end
      if not b then
        Message("&eUnknown blocktype '"..args[2].."'."):Send(player)
        return
      end
    end
    PlayerSetNextBlock(player,SphereBlock,"Drawing sphere",b,radius)
    Message("&ePlace the center block."):Send(player)
  end
)

function SphereBlock(player,args,holding,block,radius)
  block = block or Blocktype.FindById(holding)
  local x,y,z = args.X,args.Y,args.Z
  Message("&eSphere drawn: "..block.Name.." "..x..","..y..","..z..":"..radius.."."):Send(player)
  player.Level:Sphere(player,x,y,z,radius,block.ID)
  args.Abort = true
end

dofile "scripts/commands/sphere_hollow.lua"
