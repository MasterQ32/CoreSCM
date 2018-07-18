

db = { }


f = io.open("pinconfig-lpc15xx.csv", "r")
while true do
  l = f:read("*line")
  if not l then
    break
  end

  n,LBGA256,TFBGA100,LQFP144,LQFP208,preset,ptype,desc = l:match("^([\"A-Za-z0-9_]+)%s*,%s*(\"?%w*)")
  if n and n ~= '""' then
    n = n:gsub("\"", "")
    cur = { }
    db[#db+1] = cur
    print(n,LBGA256, TFBGA100, LQFP144, LQFP208)
  end

end

print("count:", #db)
