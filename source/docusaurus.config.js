// @ts-check
import {themes as prismThemes} from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'ManuAPI',
  tagline: 'A modding framework for Classic Us',
  favicon: 'img/logo.png',

  url: 'https://techdevofficial.github.io',
  baseUrl: '/ClassicUs.ManuAPI/',

  organizationName: 'TechDevOfficial',
  projectName: 'ClassicUs.ManuAPI',

  onBrokenLinks: 'warn',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: './sidebars.js',
          routeBasePath: '/',
          editUrl: 'https://github.com/TechDevOfficial/ClassicUs.ManuAPI/tree/main/docs/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      colorMode: {
        defaultMode: 'dark',
        respectPrefersColorScheme: true,
      },
      navbar: {
        title: 'ManuAPI',
        logo: {
          alt: 'ManuAPI',
          src: 'img/logo.png',
          srcDark: 'img/logo.png',
          width: 32,
          height: 32,
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'tutorialSidebar',
            position: 'left',
            label: 'Docs',
          },
          {
            href: 'https://github.com/TechDevOfficial/ClassicUs.ManuAPI',
            label: 'GitHub',
            position: 'right',
          },
          {
            href: 'https://github.com/TechDevOfficial/ClassicUs.Manactor',
            label: 'Manactor',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {label: 'Overview', to: '/'},
              {label: 'Custom Roles', to: '/roles/custom-roles'},
              {label: 'Ability Buttons', to: '/abilities/ability-buttons'},
              {label: 'Kill Manager', to: '/kills/kill-manager'},
            ],
          },
          {
            title: 'Ecosystem',
            items: [
              {label: 'Manactor', href: 'https://github.com/TechDevOfficial/ClassicUs.Manactor'},
              {label: 'GameLibs', href: 'https://github.com/TechDevOfficial/ClassicUs.GameLibs'},
            ],
          },
          {
            title: 'More',
            items: [
              {label: 'GitHub', href: 'https://github.com/TechDevOfficial/ClassicUs.ManuAPI'},
            ],
          },
        ],
        copyright: `ManuAPI — built for Classic Us. Runs on top of Manactor.`,
      },
      prism: {
        theme: prismThemes.oneLight,
        darkTheme: prismThemes.oneDark,
        additionalLanguages: ['csharp', 'xml-doc'],
      },
    }),
};

export default config;
