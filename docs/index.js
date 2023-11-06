var input_device_name = document.getElementById("input_device_name")
var input_device_display_name = document.getElementById("input_device_display_name")
var input_device_brand = document.getElementById("input_device_brand")

var input_width = document.getElementById("input_width")
var input_height = document.getElementById("input_height")

var input_range = document.getElementById("input_range");

var canvas = document.getElementById("canvas")

var rgb_width = 0, rgb_height = 0;
var image_data = null;

var drop_finish = false;
var canvas_size = 50;

input_width.addEventListener('wheel', doNumberWheel, { passive: false })
input_height.addEventListener('wheel', doNumberWheel, { passive: false })

input_range.oninput = () => {
    canvas_size = (input_range.value / 100) * 50;

    console.log(document.documentElement.style.getPropertyValue('--model-text-font'))

    document.documentElement.style.setProperty('--model-text-font', `${parseInt(canvas_size / 50 * 20)}px`);

    updateCanvas(RGB_Infos);
}

input_width.oninput = doNumberInput;
input_height.oninput = doNumberInput;

// [x, y, index]

const RGB_Infos = [];

function doNumberInput(handler, event) {
    let value = handler.target.value.replace(/[^\d]/g, '0');
    handler.target.value = value;

    updateCanvas();
}

function doNumberWheel(event) {
    event.preventDefault();

    let value = parseInt(event.target.value);

    if (event.target.value == '') {
        value = 0;
    }

    /* 下滑 */
    if (event.deltaY > 0) {
        if (value != 0) {
            event.target.value = value - 1;
        }
    }
    /* 上滑 */
    else if (event.deltaY < 0) {
        event.target.value = value + 1;
    }

    updateCanvas();
}

function updateCanvas(RGB_pos = null) {
    rgb_width = parseInt(input_width.value);
    rgb_height = parseInt(input_height.value);

    if (rgb_height == 0 || rgb_width == 0) {
        canvas.parentElement.classList.add("hidden");
        canvas.innerHTML = "";

        return;
    }

    if (RGB_pos == null)
        RGB_Infos.splice(0, RGB_Infos.length);

    let result = [];

    result.push(`<div class="index_box" style="width:${canvas_size}px;height:${canvas_size}px">
                        <label class="item">
                            <p class="number"></p>
                        </label>
                    </div>`);

    for (let column = 0;column < rgb_width;column++) {
        result.push(`<div class="index_box" style="width:${canvas_size}px;height:${canvas_size}px">
                        <label class="item">
                            <p class="number">${column}</p>
                        </label>
                    </div>`);
    }

    for (let row = 0;row < rgb_height;row++) {
        result.push(`<div class="index_box" style="width:${canvas_size}px;height:${canvas_size}px">
                        <label class="item">
                            <p class="number">${row}</p>
                        </label>
                    </div>`);

        for (let column = 0;column < rgb_width;column++) {

            let is_checked = '';
            let number = '';
            let draggable = '';

            if (RGB_pos != null) {
                for (let i = 0;i < RGB_pos.length;i++) {
                    if (RGB_pos[i][0] == column && RGB_pos[i][1] == row) {
                        is_checked = 'checked';
                        number = i;
                        draggable = 'draggable=true';

                        break;
                    }
                }
            }

            result.push(
                `<div class="led_box" style="width:${canvas_size}px;height:${canvas_size}px" ${draggable}>
                    <input onclick=checkboxHandler(this) type="checkbox" class="checkbox" id=box${column},${row} required ${is_checked}/>
                    <label class="item" for=box${column},${row}>
                        <p class="number">${number}</p>
                    </label>
                </div>`)
        }
    }

    canvas.parentElement.classList.remove("hidden");

    canvas.innerHTML = result.join('');
    canvas.style.width = `${(rgb_width + 1) * canvas_size}px`;
    
    for (let i = 0;i < canvas.childElementCount;i++) {
        if (canvas.children[i].classList.contains("led_box")) {
            addAllListener(canvas.children[i]);
        }
    }
}

function checkboxHandler(checkbox) {
    // checkbox.checked = false;

    let Number = getNumberByCheckbox(checkbox);

    if (checkbox.checked) {
        let pos_string = String(checkbox.id.substring(3));
        pos_string = pos_string.split(",")

        let RGB_Info = [parseInt(pos_string[0]), parseInt(pos_string[1]), RGB_Infos.length];
        Number.innerHTML = RGB_Infos.length;

        RGB_Infos.push(RGB_Info);

        checkbox.parentElement.draggable = true;
    }
    else {
        if (parseInt(Number.innerHTML) == RGB_Infos.length - 1) {
            checkbox.parentElement.draggable = false;
            
            Number.innerHTML = "";
            RGB_Infos.pop();
        }
        else {
            checkbox.checked = true;
        }
    }
}

// 通过Checkbox获取
function getNumberByCheckbox(checkbox) {
    let led_box = checkbox.parentElement;
    return led_box.getElementsByClassName("number")[0];
}

// 通过最上级div获取
function getNumberByElement(div) {
    let checkbox = div.getElementsByClassName("checkbox")[0];
    return getNumberByCheckbox(checkbox);
}

// 通过最上级div获取CheckBox
function getCheckboxByElement(div) {
    return div.getElementsByClassName("checkbox")[0];
}

// 通过最上级div获取CheckBox
function getLabelByElement(div) {
    return div.getElementsByClassName("item")[0];
}

function addAllListener(element) {
    element.addEventListener('dragstart', function(event) {
        
        let Number = getNumberByElement(element);
        let index = parseInt(Number.innerHTML)

        drop_finish = false;

        event.dataTransfer.setData('application/index', index);
    });

    element.addEventListener('dragend', function(event) {
        event.preventDefault();

        let label = getLabelByElement(element);
        label.style.backgroundColor = ''

        if (drop_finish) {
            let checkbox = getCheckboxByElement(element);
            let Number = getNumberByCheckbox(checkbox);

            checkbox.checked = false;    
            Number.innerHTML = '';
        }
    });

    element.addEventListener('dragleave', function(event) {
        event.preventDefault();

        let label = getLabelByElement(element);
        label.style.backgroundColor = '';
    });

    element.addEventListener('dragover', function(event) {
        event.preventDefault();

        let checkbox = getCheckboxByElement(element);
        let label = getLabelByElement(element);

        if (checkbox.checked) {
            label.style.backgroundColor = '#ff3333'
        }
        else {
            label.style.backgroundColor = '#f39c12';
        }
    })

    element.addEventListener('drop', function(event) {
        event.preventDefault();

        let index = event.dataTransfer.getData('application/index');
        let label = getLabelByElement(element);
        let checkbox = getCheckboxByElement(element);
        let number = getNumberByCheckbox(checkbox);

        label.style.backgroundColor = ''

        if(index == number.innerHTML || checkbox.checked) {
            return;
        }

        drop_finish = true;

        number.innerHTML = index;
        checkbox.checked = true;

        // 获取索引位置
        let rgb_index = RGB_Infos.findIndex(item => item[2] == index);

        // 获取新位置信息
        let pos_string = String(checkbox.id.substring(3));
        pos_string = pos_string.split(",")

        RGB_Infos[rgb_index] = [parseInt(pos_string[0]), parseInt(pos_string[1]), parseInt(index)];

        checkbox.parentElement.draggable = true;
    })
}

function reset() {
    updateCanvas();

    const image = document.getElementById('show_image');
    image.src = "";
    image_data = null;
}

function exportJson() {
    if (rgb_height == 0 || rgb_width == 0
        || input_device_name.value == ""
        || input_device_display_name.value == ""
        || input_device_brand.value == "") {
            alert("信息填写不完整");
            return;
        }

    if (RGB_Infos.length == 0) {
        alert("请选择至少一个灯珠");
        return;
    }

    const jsonData = {
        "ProductName": input_device_name.value,
        "DisplayName": input_device_display_name.value,
        "Brand": input_device_brand.value,
        "Type": "custom",
        "LedCount": RGB_Infos.length,
        "Width": rgb_width,
        "Height": rgb_height,
        "LedMapping": [],
        "LedCoordinates": [],
        "LedNames": []
    }

    if (image_data != null) {
        jsonData.Image = image_data;
    }

    for (let i = 0;i < RGB_Infos.length; i++) {
        jsonData.LedMapping.push(i);
        jsonData.LedCoordinates.push([RGB_Infos[i][0], RGB_Infos[i][1]]);
        jsonData.LedNames.push(`Led${i}`);
    }

    const jsonString = JSON.stringify(jsonData);

    const blob = new Blob([jsonString], { type: 'application/json' })
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = `${jsonData.DisplayName}.json`;

    a.click();

    URL.revokeObjectURL(url);
}

function importJson() {
    const input = document.createElement('input');
    input.type = "file";

    input.addEventListener('change', function(event) {
        const selectedFile = event.target.files[0];
        if (selectedFile) {
            const reader = new FileReader();

            reader.onload = function(event) {
                const fileContents = event.target.result;
                let json = JSON.parse(fileContents);

                try {
                    input_device_name.value = json["ProductName"];
                    input_device_display_name.value = json["DisplayName"];
                    input_device_brand.value = json["Brand"];

                    input_width.value = json["Width"];
                    input_height.value = json["Height"];

                    let RGB_Pos = json["LedCoordinates"];
                    updateCanvas(RGB_Pos);

                    RGB_Infos.splice(0, RGB_Infos.length);

                    for (let i = 0;i < RGB_Pos.length;i++) {
                        RGB_Infos.push([RGB_Pos[i][0], RGB_Pos[i][1], i]);
                    }

                    image_data = json["Image"];

                    const image = document.getElementById('show_image');
                    image.src = `data:image/png;base64,${image_data}`;
                }
                catch {
                    alert("读取Json文件错误");

                    reset();
                }
            }

            reader.readAsText(selectedFile);
        }
    });

    input.click();
}

function drop_image(event) {
    event.preventDefault();
    event.stopPropagation();

    const data = event.dataTransfer;

    if (data.items) {
        if (data.items[0].type === 'image/png') {
            if (data.items[0].kind === 'file') {
                const file = data.items[0].getAsFile();
                const reader = new FileReader();
    
                reader.onload = function(e) {
                    const image = document.getElementById('show_image');
                    image.src = e.target.result;

                    image_data = image.src.replace('data:image/png;base64,', '');
                };
    
                reader.readAsDataURL(file);
            }
        }
        else {
            alert("错误的类型");
        }
    }
}

function online() {
    location.assign("https://hyydsz.github.io/MakeRGB/");
}

onload = () => {
    updateCanvas();
}