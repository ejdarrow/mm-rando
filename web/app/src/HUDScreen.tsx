import React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMoon, faSun } from '@fortawesome/free-regular-svg-icons';
import './HUDScreen.css';

const id = {
  buttonA: 'button-a',
  buttonB: 'button-b',
  buttonC: 'button-c',
  buttonStart: 'button-start',
  clockEmblemInverted1: 'clock-emblem-inverted-1',
  clockEmblemInverted2: 'clock-emblem-inverted-2',
  clockEmblemSun: 'clock-emblem-sun',
  clockEmblemUninverted: 'clock-emblem-uninverted',
  clockMoon: 'clock-moon',
  clockSun: 'clock-sun',
  dpad: 'dpad',
  hearts: 'hearts',
  heartsDoubleDefense: 'hearts-dd',
  magic: 'magic',
  magicInfinite: 'magic-inf',
  map: 'map',
  mapCursorEntrance: 'map-cursor-entrance',
  mapCursorPlayer: 'map-cursor-player',
  rupee1: 'rupee-1',
  rupee2: 'rupee-2',
  rupee3: 'rupee-3',
  rupee4: 'rupee-4',
};

interface HUDScreenProps {
  onClick: (event: React.MouseEvent<HTMLElement, MouseEvent>, identifier: string) => void;
}

class HUDScreen extends React.Component<HUDScreenProps> {
  render() {
    console.log('HUDScreen::render()');
    return (
      <div
        className="bg-darker-1/4 border-2 flex flex-col font-sans grow p-4 select-none h-[56rem] max-h-[56rem] min-h-[30rem] min-w-[60rem] max-w-full"
        data-hud="root"
      >
        <div className="flex">
          {/* Heart colors, magic bar colors, D-Pad color. */}
          <div>
            <fieldset className="border-2 border-dashed p-4" style={{ marginTop: '-0.5rem' }}>
              <legend className="px-3">
                <span>Hearts &amp; Magic</span>
              </legend>
              <div className="flex flex-col gap-3 w-80">
                <button
                  className="text-3xl text-center my-outline"
                  data-hud={id.hearts}
                  data-hud-effect="text"
                  onClick={(event) => this.props.onClick(event, id.hearts)}
                >
                  <span className="my-text-shadow-border-black">♥ ♥ ♥ ♥ ♥ ♥ ♥ ♥ ♥ ♥</span>
                </button>
                <button
                  className="text-3xl text-center my-outline"
                  data-hud={id.heartsDoubleDefense}
                  data-hud-effect="text"
                  onClick={(event) => this.props.onClick(event, id.heartsDoubleDefense)}
                >
                  <span className="my-text-shadow-border-white">♥ ♥ ♥ ♥ ♥ ♥ ♥ ♥ ♥ ♥</span>
                </button>
                <button
                  className="my-outline-with-thin-border h-6 w-1/2"
                  data-hud={id.magic}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.magic)}
                ></button>
                <button
                  className="my-outline-with-thin-border h-6"
                  data-hud={id.magicInfinite}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.magicInfinite)}
                ></button>
              </div>
            </fieldset>
            <div className="h-4"></div>
            <button
              className="my-outline-with-border m-2px h-20 w-20"
              data-hud={id.dpad}
              data-hud-effect="background"
              onClick={(event) => this.props.onClick(event, id.dpad)}
            >
              <span className="my-bubble">D-Pad</span>
            </button>
          </div>
          {/* Start button color. */}
          <div className="flex place-content-center grow">
            <button
              className="my-outline-with-border m-2px h-16 w-20"
              data-hud={id.buttonStart}
              data-hud-effect="background"
              onClick={(event) => this.props.onClick(event, id.buttonStart)}
            >
              <span className="my-bubble">Start</span>
            </button>
          </div>
          {/* A, B, C button colors. */}
          <div>
            <fieldset className="border-2 border-dashed p-4" style={{ marginTop: '-0.5rem' }}>
              <legend className="px-3">
                <span>Buttons</span>
              </legend>
              <div className="flex gap-4 w-80">
                <button
                  className="my-outline-with-border h-16 w-16"
                  data-hud={id.buttonB}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.buttonB)}
                >
                  <span className="my-bubble">B</span>
                </button>
                <button
                  className="my-outline-with-border h-16 w-16 mt-8"
                  data-hud={id.buttonA}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.buttonA)}
                >
                  <span className="my-bubble">A</span>
                </button>
                <button
                  className="my-outline-with-border h-16 grow"
                  data-hud={id.buttonC}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.buttonC)}
                >
                  <span className="my-bubble">C</span>
                </button>
              </div>
            </fieldset>
          </div>
        </div>
        <div style={{ maxHeight: '8rem', minHeight: '1rem' }}></div>
        <div className="flex grow">
          {/* Rupee icon colors. */}
          <div className="flex flex-col gap-2 justify-end w-80">
            <fieldset className="border-2 border-dashed flex flex-col gap-3 p-4 w-max">
              <legend className="px-3">
                <span>Rupees</span>
              </legend>
              <div className="flex gap-2 items-center">
                <button
                  className="my-outline-with-thin-border h-10 w-10"
                  data-hud={id.rupee4}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.rupee4)}
                ></button>
                <div>
                  <span className="my-bubble">999</span>
                </div>
              </div>
              <div className="flex gap-2 items-center">
                <button
                  className="my-outline-with-thin-border h-10 w-10"
                  data-hud={id.rupee3}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.rupee3)}
                ></button>
                <div>
                  <span className="my-bubble">500</span>
                </div>
              </div>
              <div className="flex gap-2 items-center">
                <button
                  className="my-outline-with-thin-border h-10 w-10"
                  data-hud={id.rupee2}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.rupee2)}
                ></button>
                <div>
                  <span className="my-bubble">200</span>
                </div>
              </div>
              <div className="flex gap-2 items-center">
                <button
                  className="my-outline-with-thin-border h-10 w-10"
                  data-hud={id.rupee1}
                  data-hud-effect="background"
                  onClick={(event) => this.props.onClick(event, id.rupee1)}
                ></button>
                <div>
                  <span className="my-bubble">99</span>
                </div>
              </div>
            </fieldset>
          </div>
          {/* Clock colors. */}
          <div className="flex gap-2 grow items-end place-content-center">
            <fieldset className="border-2 border-dashed flex gap-3 items-end place-content-center p-4">
              <legend className="px-3">
                <span>Clock</span>
              </legend>
              <button
                className="my-outline-with-border flex-center h-12 w-12"
                data-hud={id.clockSun}
                data-hud-effect="text"
                onClick={(event) => this.props.onClick(event, id.clockSun)}
              >
                <FontAwesomeIcon className="text-2xl my-text-shadow-border-darker" icon={faSun} />
              </button>
              <div
                className="my-outline-with-thin-border cursor-pointer flex-center p-6 h-52 w-48"
                data-hud="clock-emblem-sun"
              >
                <div className="my-outline-with-thin-border-inverted divide-y flex flex-col h-full w-full">
                  <button
                    className="grow"
                    data-hud={id.clockEmblemInverted2}
                    data-hud-effect="background"
                    onClick={(event) => this.props.onClick(event, id.clockEmblemInverted2)}
                  >
                    <span className="my-bubble">Inverted B</span>
                  </button>
                  <button
                    className="grow"
                    data-hud={id.clockEmblemInverted1}
                    data-hud-effect="background"
                    onClick={(event) => this.props.onClick(event, id.clockEmblemInverted1)}
                  >
                    <span className="my-bubble">Inverted A</span>
                  </button>
                  <button
                    className="grow"
                    data-hud={id.clockEmblemUninverted}
                    data-hud-effect="background"
                    onClick={(event) => this.props.onClick(event, id.clockEmblemUninverted)}
                  >
                    <span className="my-bubble">Uninverted</span>
                  </button>
                </div>
              </div>
              <button
                className="my-outline-with-border flex-center h-12 w-12"
                data-hud={id.clockMoon}
                data-hud-effect="text"
                onClick={(event) => this.props.onClick(event, id.clockMoon)}
              >
                <FontAwesomeIcon className="text-2xl my-text-shadow-border-darker" icon={faMoon} />
              </button>
            </fieldset>
          </div>
          {/* Minimap colors. */}
          <div className="flex flex-row-reverse items-end w-80">
            <fieldset className="border-2 border-dashed p-4">
              <legend className="px-3">
                <span>Minimap</span>
              </legend>
              <div className="flex flex-col-reverse gap-3 items-center">
                <div
                  className="my-outline-with-thin-border cursor-pointer flex items-end justify-center p-4 h-32 w-32"
                  data-hud={id.map}
                  onClick={(event) => this.props.onClick(event, id.map)}
                >
                  <button
                    className="my-outline-with-thin-border-inverted h-10 w-10"
                    data-hud={id.mapCursorPlayer}
                    onClick={(event) => this.props.onClick(event, id.mapCursorPlayer)}
                  >
                    <span className="text-2xl my-text-shadow-border-darker">▲</span>
                  </button>
                </div>
                <button
                  className="my-outline-with-border h-10 w-10"
                  data-hud={id.mapCursorEntrance}
                  onClick={(event) => this.props.onClick(event, id.mapCursorEntrance)}
                >
                  <span className="text-2xl my-text-shadow-border-darker">▼</span>
                </button>
              </div>
            </fieldset>
          </div>
        </div>
      </div>
    );
  }
}

export default HUDScreen;
