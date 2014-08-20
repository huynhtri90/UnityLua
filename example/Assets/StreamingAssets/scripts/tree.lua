﻿

function Tree(var, depth)

	local treeData =
	{
		tableVisit = {},
		thisPath = "*",
		Output = function (self, textLine)
            Log(textLine);
		end
	};

    Log("*");

	Show(treeData, var, nil, depth);
end

function PrintTree(var, depth)

	local treeData =
	{
		tableVisit = {},
		thisPath = "*",
		Output = function (self, textLine)
            print(textLine);
		end
	};

    print("*");

	Show(treeData, var, nil, depth);
end


function VarText(treeData, var)
	local varType = type(var);
	local text = "";

	if varType == "string" then
		text = "@"..var;
	elseif varType == "number" then	
		text = "#"..var;
	elseif varType == "boolean" then	
		text = "!"..tostring(var);
	elseif varType == "table" then
		local path = treeData.tableVisit[var];
		if path then
			text = "$("..path..")";
		else
			text = "$("..tostring(var)..")";
		end
	else
		text = "?"..tostring(var);
	end

	text = string.gsub(text, "\r", "\\r");
	text = string.gsub(text, "\n", "\\n");
	text = string.gsub(text, "%c", "\\^");		

	return text;
end

function Show(treeData, var, szBase, depth)
	if depth and depth <= 0 then
		return;
	end

    if not szBase then
        szBase = "";
    end

    if type(var) ~= "table" then
    	treeData:Output(VarText(treeData, var));
		return;
	end

	treeData.tableVisit[var] = treeData.thisPath;
    
	local nSize  = 0;	-- table中一共有多少项
	local nCount = 1;	-- 当前遍历到的是第几项
    
    for k, v in pairs(var) do
        nSize = nSize + 1;
    end
	
	for k, v in pairs(var) do		
    	local visitPath = nil;
		local szNode;
        local vType = type(v);

        if vType == "table" then
        	visitPath = treeData.tableVisit[v];
        	if visitPath then
        		szNode = VarText(treeData, k)..": "..VarText(treeData, v);	
        	else
            	szNode = VarText(treeData, k);
        	end
        else
            szNode = VarText(treeData, k)..": "..VarText(treeData, v);
        end
		
		if nCount < nSize then
			treeData:Output(""..szBase.."├─"..szNode.."");
		else
			treeData:Output(""..szBase.."└─"..szNode.."");
		end
		
	    if vType == "table" and not visitPath then
	    	local myPath = treeData.thisPath;

	    	treeData.thisPath = treeData.thisPath.."/"..VarText(treeData, k);

			if nCount < nSize then
				Show(treeData, v, ""..szBase.."│  ", depth and (depth - 1));
			else 
				Show(treeData, v, ""..szBase.."   ", depth and (depth - 1));
			end

			treeData.thisPath = myPath;
		end	

        nCount = nCount + 1;
	end	
end

--------测试数据--------
--Log("_G");
--Tree(_G);

player =
{
    name = "张三",
    id = 123,
    version = 1000,
    last_login_ip = "172.16.11.152",
    last_login_time = os.time(),
    data = {},
    login = function()
    	print("");
    end
}

card1 = {id = "00_17_55_7f", level = 3, template = 100, owner = player};
card2 = {id = "75_82_35_1e", level = 1, template = 17};
card3 = {id = "97_21_30_7f", level = 2, template = 99};

player.data.cards = {card1, card2, card1};

count = count or 1;
--print("--------------------------"..count.."------------------------------");
count = count + 1;

--Log("player");
--Tree(player);
