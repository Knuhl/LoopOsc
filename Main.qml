import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.3
import LoopMachineOsc 1.0

ApplicationWindow {
  id: window
  width: 400
  height: 200
  visible: true
  title: "TestWindow"

  Column {

    Row {
      Repeater {
        id: repeater
        Column {
          width: 50
          Text { text: modelData.title }
          Column {
            height: 50;
            Text { text: "R"; color: "red"; visible: modelData.redLedVisible; }
            Text { text: "G"; color: "green"; visible: modelData.greenLedVisible; }
          }
          Button {
            text: "o"
            width: 40
            onClicked: {
              vm.press(modelData.buttonType)
            }
          }
        }
      }
    }
  }

  function updateButtons()
  {
    repeater.model = Net.toListModel(vm.buttons)
  }

  MainViewModel {
    id: vm
    Component.onCompleted: {
      vm.buttonsChanged.connect(updateButtons);
      updateButtons();
    }
  }
}
